using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Solitaire.Models;
using Solitaire.Utils;
using Solitaire.ViewModels.Pages;

namespace Solitaire.ViewModels;

/// <summary>
/// Base class for a ViewModel for a card game.
/// </summary>
public abstract partial class CardGameViewModel : ViewModelBase
{
    public ImmutableArray<PlayingCardViewModel>? Deck;

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
        if (_moveStack.Count > 0)
        {
            var operations = _moveStack.Pop();

            foreach (var operation in operations)
            {
                operation.Revert(this);
            }

           
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CardGameViewModel"/> class.
    /// </summary>
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

        UndoCommand = new RelayCommand(UndoMove);

        DoInitialize();
    }

    private void DoInitialize()
    {
        //  Set up the timer.
        _timer.Interval = TimeSpan.FromMilliseconds(500);
        _timer.Tick += timer_Tick;
        InitializeDeck();
    }

    protected virtual void InitializeDeck()
    {
        if (Deck is { }) return;

        var playingCards = Enum
            .GetValuesAsUnderlyingType(typeof(CardType))
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


    public abstract IList<PlayingCardViewModel>? GetCardCollection(PlayingCardViewModel card);


    public abstract bool CheckAndMoveCard(IList<PlayingCardViewModel> from,
        IList<PlayingCardViewModel> to,
        PlayingCardViewModel card,
        bool checkOnly = false);

    /// <summary>
    /// Deals a new game.
    /// </summary>
    protected void ResetInternalState()
    {
        ClearUndoStack();
        
        //  Stop the timer and reset the game data.
        StopTimer();
        ElapsedTime = TimeSpan.FromSeconds(0);
        Moves = 0;
        Score = 0;
        IsGameWon = false;
        OnPropertyChanged(nameof(IsGameWon));
    }

    /// <summary>
    /// Starts the timer.
    /// </summary>
    protected void StartTimer()
    {
        _lastTick = DateTime.Now;
        _timer.Start();
    }

    /// <summary>
    /// Stops the timer.
    /// </summary>
    protected void StopTimer()
    {
        _timer.Stop();
    }

    /// <summary>
    /// Handles the Tick event of the timer control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void timer_Tick(object? sender, EventArgs e)
    {
        //  Get the time, update the elapsed time, record the last tick.
        var timeNow = DateTime.Now;
        ElapsedTime += timeNow - _lastTick;
        _lastTick = timeNow;
    }

    /// <summary>
    /// Fires the game won event.
    /// </summary>
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

    /// <summary>
    /// Gets the go to casino command.
    /// </summary>
    /// <value>The go to casino command.</value>
    public ICommand? NavigateToCasinoCommand { get; }

    /// <summary>
    /// Gets the deal new game command.
    /// </summary>
    /// <value>The deal new game command.</value>
    public ICommand? NewGameCommand { get; protected set; }

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

    public class FlipOperation : CardOperation
    {
        public FlipOperation(PlayingCardViewModel flipped)
        {
            Flipped = flipped;
        }
        
        public PlayingCardViewModel Flipped { get; }
        
        public override void Revert(CardGameViewModel game)
        {
            Flipped.IsFaceDown = !Flipped.IsFaceDown;
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