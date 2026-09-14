using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Threading;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Solitaire.Models;
using Solitaire.Utils;
using Solitaire.ViewModels.Pages;

namespace Solitaire.ViewModels;

public abstract partial class CardGameViewModel : ViewModelBase
{
    public ImmutableArray<PlayingCardViewModel>? Deck { get; set; }

    public ICommand? AutoMoveCommand { get; protected set; }

    private readonly Stack<CardOperation[]> _moveStack = new();

    public abstract string? GameName { get; }

    private void ClearUndoStack()
    {
        _moveStack.Clear();
    }



    protected void RecordMoves(params CardOperation[] operations)
    {
        _moveStack.Push(operations);
    }

    private void UndoMove()
    {
        if (!CanUndo)
            return;
        if (_moveStack.Count > 0)
        {
            var operations = _moveStack.Pop();

            foreach (var operation in operations)
            {
                operation.Revert(this);
            }


        }
    }

    protected virtual bool CanUndo => true;

    protected CardGameViewModel(CasinoViewModel casinoViewModel)
    {
        NavigateToCasinoCommand =
            new RelayCommand(() =>
            {
                if (Moves > 0)
                {
                    _gameStats.UpdateStatistics();
                    casinoViewModel.Save();
                }

                casinoViewModel.CurrentView = casinoViewModel.TitleInstance;
            });

        UndoCommand = new RelayCommand(UndoMove, () => CanUndo);

        DoInitialize();
    }

    private void DoInitialize()
    {
        _timer.Interval = TimeSpan.FromMilliseconds(500);
        _timer.Tick += timer_Tick;
        InitializeDeck();
    }

    protected virtual void InitializeDeck()
    {
        if (Deck is { })
            return;

        var playingCards = Enum
            .GetValuesAsUnderlyingType<CardType>()
            .Cast<CardType>()
            .Select(cardType => new PlayingCardViewModel(this)
            { CardType = cardType, IsFaceDown = true })
            .ToImmutableArray();

        Deck = playingCards;
    }

    protected IList<PlayingCardViewModel> GetNewShuffledDeck()
    {
        foreach (var card in Deck!)
        {
            card.Reset();
        }

        var playingCards = Deck.Value.OrderBy(_ => PlatformProviders.NextRandomDouble()).ToList();

        return playingCards.Count == 0
            ? throw new InvalidOperationException("Starting deck cannot be empty.")
            : playingCards;
    }


    protected static async Task<bool> MoveTableauCardsToFoundations(
        IEnumerable<BatchObservableCollection<PlayingCardViewModel>> tableaus,
        Func<PlayingCardViewModel, bool> tryMove, CancellationToken cancellation)
    {
        var moved = false;
        foreach (var tableau in tableaus)
        {
            if (cancellation.IsCancellationRequested)
                return false;
            if (tableau.Count > 0 && tryMove(tableau.Last()))
            {
                moved = true;
                if (!await PauseGameAction(75, cancellation))
                    return false;
            }
        }
        return moved;
    }

    public abstract IList<PlayingCardViewModel>? GetCardCollection(PlayingCardViewModel card);


    public abstract bool CheckAndMoveCard(IList<PlayingCardViewModel> from,
        IList<PlayingCardViewModel> destination,
        PlayingCardViewModel card,
        bool checkOnly = false);

    protected void ResetInternalState()
    {
        ResetPendingActions();
        ClearUndoStack();

        StopTimer();
        ElapsedTime = TimeSpan.FromSeconds(0);
        Moves = 0;
        Score = 0;
        IsGameWon = false;
        OnPropertyChanged(nameof(IsGameWon));
    }

    protected void StartTimer()
    {
        _lastTick = DateTime.Now;
        _timer.Start();
    }

    protected void StopTimer()
    {
        _timer.Stop();
    }

    private void timer_Tick(object? sender, EventArgs e)
    {
        var timeNow = DateTime.Now;
        ElapsedTime += timeNow - _lastTick;
        _lastTick = timeNow;
    }

    protected void FireGameWonEvent()
    {
        _gameStats.UpdateStatistics();

        var wonEvent = GameWon;
        if (wonEvent is not { })
            wonEvent?.Invoke();
    }

    /// <summary>
    /// The timer for recording the time spent in a game.
    /// </summary>
    private readonly DispatcherTimer _timer = new();

    /// <summary>
    /// The time of the last tick.
    /// </summary>
    private DateTime _lastTick;

    [ObservableProperty] private int _score;

    [ObservableProperty] private TimeSpan _elapsedTime;

    [ObservableProperty] private int _moves;

    [ObservableProperty] private bool _isGameWon;
    private GameStatisticsViewModel _gameStats = null!;
    internal GameStatisticsViewModel GameStatistics => _gameStats;

    public ICommand? NavigateToCasinoCommand { get; }

    public ICommand? NewGameCommand { get; protected internal set; }

    public ICommand? UndoCommand { get; protected set; }

    /// <summary>
    /// Occurs when the game is won.
    /// </summary>
    public event Action GameWon = null!;

    public abstract void ResetGame();

    public void RegisterStatsInstance(GameStatisticsViewModel gameStatsInstance)
    {
        _gameStats = gameStatsInstance;
    }

    public abstract class CardOperation
    {
        public abstract void Revert(CardGameViewModel game);
    }

    public class GenericOperation : CardOperation
    {
        private readonly Action _action;

        public GenericOperation(Action action)
        {
            _action = action;
        }

        public override void Revert(CardGameViewModel game)
        {
            _action();
        }
    }

    public class MoveOperation : CardOperation
    {
        public MoveOperation(IList<PlayingCardViewModel> from, IList<PlayingCardViewModel> to, IList<PlayingCardViewModel> run,
            int score)
        {
            From = from;
            To = to;
            Run = run;
            Score = score;
        }

        public IList<PlayingCardViewModel> From { get; }

        public IList<PlayingCardViewModel> To { get; }

        public IList<PlayingCardViewModel> Run { get; }

        public int Score { get; }

        public override void Revert(CardGameViewModel game)
        {

            game.Score -= Score;

            foreach (var runCard in Run)
                From.Add(runCard);
            foreach (var runCard in Run)
                To.Remove(runCard);

            game.Moves--;
        }
    }
}
