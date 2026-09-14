using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Solitaire.Models;
using Solitaire.Utils;

namespace Solitaire.ViewModels.Pages;

public partial class KlondikeSolitaireViewModel : CardGameViewModel
{
    /// <inheritdoc />
    public override string GameName => "Klondike";

    [ObservableProperty] private DrawMode _drawMode;
    private bool _isTurning;
    private bool _isDealing;
    private int _gameVersion;

    private bool CanMoveCards => !_isTurning && !_isDealing && !IsGameWon;
    private bool CanTurnStock() => CanMoveCards && AutoMoveCommand is not IAsyncRelayCommand { IsRunning: true };
    protected override bool CanUndo => CanTurnStock();

    private void NotifyGameCommands()
    {
        (TurnStockCommand as IRelayCommand)?.NotifyCanExecuteChanged();
        (AutoMoveCommand as IRelayCommand)?.NotifyCanExecuteChanged();
        (UndoCommand as IRelayCommand)?.NotifyCanExecuteChanged();
    }

    public KlondikeSolitaireViewModel(CasinoViewModel casinoViewModel) : base(casinoViewModel)
    {
        _casinoViewModel = casinoViewModel;
        InitializeFoundationsAndTableauSet();

        TurnStockCommand = new AsyncRelayCommand(DoTurnStock, CanTurnStock);
        var autoMove = new AsyncRelayCommand(TryMoveAllCardsToAppropriateFoundations, () => CanMoveCards);
        autoMove.PropertyChanged += OnAutoMoveStateChanged;
        AutoMoveCommand = autoMove;
        NewGameCommand = new AsyncRelayCommand(DoDealNewGame);
    }

    private void OnAutoMoveStateChanged(object? sender, PropertyChangedEventArgs change)
    {
        if (change.PropertyName == nameof(AsyncRelayCommand.IsRunning))
            NotifyGameCommands();
    }

    private void InitializeFoundationsAndTableauSet()
    {
        _foundations.Add(Foundation1);
        _foundations.Add(Foundation2);
        _foundations.Add(Foundation3);
        _foundations.Add(Foundation4);
        _tableauSet.Add(Tableau1);
        _tableauSet.Add(Tableau2);
        _tableauSet.Add(Tableau3);
        _tableauSet.Add(Tableau4);
        _tableauSet.Add(Tableau5);
        _tableauSet.Add(Tableau6);
        _tableauSet.Add(Tableau7);
    }

    public override IList<PlayingCardViewModel>? GetCardCollection(PlayingCardViewModel card)
    {
        if (Stock.Contains(card))
            return Stock;
        if (Waste.Contains(card))
            return Waste;

        foreach (var foundation in _foundations.Where(foundation => foundation.Contains(card)))
            return foundation;

        return _tableauSet.FirstOrDefault(tableau => tableau.Contains(card));
    }



    private async Task DoDealNewGame()
    {
        ResetGame();
        var version = _gameVersion;
        _isDealing = true;
        NotifyGameCommands();
        try
        {
            await DealCards(version);
        }
        finally
        {
            if (version == _gameVersion)
            {
                _isDealing = false;
                NotifyGameCommands();
            }
        }
    }

    private async Task DealCards(int version)
    {
        var cancellation = ActionCancellation;

        var playingCards = GetNewShuffledDeck();

        using (var stock0 = Stock.DelayNotifications())
        {
            stock0.AddRange(playingCards);
        }

        if (!await PauseGameAction(600, cancellation))
            return;
        if (version != _gameVersion)
            return;

        using (var stock0 = Stock.DelayNotifications())
        {
            stock0.Clear();
        }

        for (var i = 0; i < 7; i++)
        {
            var tempTableau = new List<PlayingCardViewModel>();

            // Each column has i face-down cards and one face-up card.
            for (var j = 0; j < i; j++)
            {
                var faceDownCardViewModel = playingCards.First();
                playingCards.Remove(faceDownCardViewModel);
                faceDownCardViewModel.IsFaceDown = true;
                tempTableau.Add(faceDownCardViewModel);
            }

            var faceUpCardViewModel = playingCards.First();
            playingCards.Remove(faceUpCardViewModel);
            faceUpCardViewModel.IsFaceDown = false;
            faceUpCardViewModel.IsPlayable = true;
            tempTableau.Add(faceUpCardViewModel);


            foreach (var card in tempTableau)
            {
                _tableauSet[i].Add(card);

                if (!await PauseGameAction(75, cancellation))
                    return;
                if (version != _gameVersion)
                    return;
            }
        }

        foreach (var playingCard in playingCards)
        {
            playingCard.IsFaceDown = true;
            playingCard.IsPlayable = false;
        }

        using var stockD = Stock.DelayNotifications();

        stockD.AddRange(playingCards);


        StartTimer();
    }

    public override void ResetGame()
    {
        _gameVersion++;
        _isTurning = false;
        _isDealing = false;
        using var stockD = Stock.DelayNotifications();
        using var wasteD = Waste.DelayNotifications();

        DrawMode = _casinoViewModel.SettingsInstance.DrawMode;

        ResetInternalState();

        foreach (var tableau in _tableauSet)
        {
            using var tableauD = tableau.DelayNotifications();
            tableauD.Clear();
        }

        foreach (var foundation in _foundations)
        {

            using var foundationD = foundation.DelayNotifications();
            foundationD.Clear();
        }

        stockD.Clear();
        wasteD.Clear();
        NotifyGameCommands();
    }

    /// <summary>
    /// Turns cards from the stock into the waste.
    /// </summary>
    private async Task DoTurnStock()
    {
        if (!CanTurnStock() || Stock.Count + Waste.Count == 0)
            return;

        var version = _gameVersion;
        var cancellation = ActionCancellation;
        _isTurning = true;
        NotifyGameCommands();
        RecordMoves(new KlondikeUndoOperation(this, Stock, Waste));
        try
        {
            foreach (var card in Waste)
                card.IsPlayable = false;

            if (Stock.Count == 0)
            {
                while (Waste.Count > 0)
                {
                    var card = Waste[0];
                    Waste.RemoveAt(0);
                    card.IsFaceDown = true;
                    Stock.Insert(0, card);
                    if (!await PauseGameAction(175, cancellation))
                        return;
                    if (version != _gameVersion)
                        return;
                }
            }
            else
            {
                var count = System.Math.Min(Stock.Count, DrawMode == DrawMode.DrawThree ? 3 : 1);
                for (var i = 0; i < count; i++)
                {
                    var card = Stock.Last();
                    Stock.RemoveAt(Stock.Count - 1);
                    card.IsFaceDown = false;
                    card.IsPlayable = false;
                    Waste.Add(card);
                    if (!await PauseGameAction(175, cancellation))
                        return;
                    if (version != _gameVersion)
                        return;
                }
            }

            if (Waste.Count > 0)
                Waste.Last().IsPlayable = true;
        }
        finally
        {
            if (version == _gameVersion)
            {
                _isTurning = false;
                NotifyGameCommands();
            }
        }
    }

    /// <summary>
    /// Moves eligible cards to their foundations.
    /// </summary>
    private async Task TryMoveAllCardsToAppropriateFoundations()
    {
        if (!CanMoveCards)
            return;
        var version = _gameVersion;
        var cancellation = ActionCancellation;
        var moved = true;
        while (moved && version == _gameVersion && CanMoveCards)
        {
            moved = false;
            foreach (var pile in _tableauSet.Prepend(Waste))
            {
                if (version != _gameVersion || !CanMoveCards)
                    return;
                if (pile.Count == 0 || !TryMoveCardToAppropriateFoundation(pile.Last()))
                    continue;
                moved = true;
                if (!await PauseGameAction(75, cancellation))
                    return;
            }
        }
    }

    /// <summary>
    /// Moves an eligible card to a foundation.
    /// </summary>
    /// <param name="card">The card.</param>
    /// <returns>True after a successful move.</returns>
    private bool TryMoveCardToAppropriateFoundation(PlayingCardViewModel card)
    {
        if (Waste.LastOrDefault() == card)
            foreach (var foundation in _foundations)
                if (CheckAndMoveCard(Waste, foundation, card))
                    return true;

        var inTableau = false;
        var i = 0;
        for (; i < _tableauSet.Count && inTableau == false; i++)
            inTableau = _tableauSet[i].Contains(card);

        if (inTableau == false)
            return false;

        foreach (var foundation in _foundations)
            if (CheckAndMoveCard(_tableauSet[i - 1], foundation, card))
                return true;

        return false;
    }

    /// <summary>
    /// Moves the card.
    /// </summary>
    /// <param name="from">The source pile.</param>
    /// <param name="destination">The destination pile.</param>
    /// <param name="card">The card to move.</param>
    /// <param name="checkOnly">If true, check the move without moving cards.</param>
    /// <returns>True for a legal move.</returns>
    public override bool CheckAndMoveCard(IList<PlayingCardViewModel> from,
        IList<PlayingCardViewModel> destination,
        PlayingCardViewModel card,
        bool checkOnly = false)
    {
        // Check the source cards before the destination.
        if (!CanMoveCards || ReferenceEquals(from, destination) || !CanSelectRun(from, destination, card))
            return false;

        int scoreModifier;

        // Waste and tableau moves share destination rules, with different tableau scores.
        var fromWaste = ReferenceEquals(from, Waste);
        if (fromWaste || _tableauSet.Contains(from))
        {
            if (!TryGetForwardMoveScore(destination, card, fromWaste ? 5 : 0, out scoreModifier))
                return false;
        }
        else if (_foundations.Contains(from))
        {
            if (_tableauSet.Contains(destination))
            {
                if (CardMoveRules.CanPlaceOnTableau(destination, card, card.Value == 12))
                {
                    scoreModifier = -15;
                }
                else
                    return false;
            }
            else if (_foundations.Contains(destination))
            {
                if (CardMoveRules.GetFoundationSuit(destination, _foundations) != card.Suit && card.Value == 0)
                {
                    return false;
                }

                if (from.Count == 1 && destination.Count == 0 && card.Value == 0)
                {
                    scoreModifier = 0;
                }
                else
                    return false;
            }
            else
                return false;
        }
        else
            return false;

        if (checkOnly)
            return true;

        MoveCard(from, destination, card);
        Score += scoreModifier;
        Moves++;

        if (fromWaste && Waste.Count > 0)
            Waste.Last().IsPlayable = true;

        CheckForVictory();

        return true;
    }

    private bool CanSelectRun(IList<PlayingCardViewModel> from, IList<PlayingCardViewModel> destination,
        PlayingCardViewModel card)
    {
        var index = from.IndexOf(card);
        if (index < 0 || card.IsFaceDown || !card.IsPlayable)
            return false;
        if (destination.Count > 0 && destination.Last().IsFaceDown)
            return false;
        if (ReferenceEquals(from, Waste) || _foundations.Contains(from))
            return index == from.Count - 1;
        if (!_tableauSet.Contains(from) || _foundations.Contains(destination) && index != from.Count - 1)
            return false;
        for (var i = index + 1; i < from.Count; i++)
            if (from[i].IsFaceDown || from[i].Colour == from[i - 1].Colour || from[i].Value != from[i - 1].Value - 1)
                return false;
        return true;
    }

    private bool TryGetForwardMoveScore(IList<PlayingCardViewModel> destination,
        PlayingCardViewModel card, int tableauScore, out int score)
    {
        score = _foundations.Contains(destination) ? 10 : tableauScore;
        if (_foundations.Contains(destination))
            return CardMoveRules.CanBuildFoundation(destination, card, _foundations, true);
        return _tableauSet.Contains(destination) && CardMoveRules.CanPlaceOnTableau(destination, card, card.Value == 12);
    }

    private void MoveCard(IList<PlayingCardViewModel> from,
        IList<PlayingCardViewModel> to,
        PlayingCardViewModel card)
    {
        var undo = new KlondikeUndoOperation(this, from, to);
        var run = from.Skip(from.IndexOf(card)).ToArray();
        foreach (var runCard in run)
            from.Remove(runCard);
        foreach (var runCard in run)
            to.Add(runCard);
        if (from.Count > 0)
        {
            from.Last().IsFaceDown = false;
            from.Last().IsPlayable = true;
        }
        RecordMoves(undo);
    }

    private void CheckForVictory()
    {
        foreach (var foundation in _foundations)
            if (foundation.Count < 13)
                return;

        IsGameWon = true;
        NotifyGameCommands();

        StopTimer();

        FireGameWonEvent();
    }

    private readonly List<BatchObservableCollection<PlayingCardViewModel>> _foundations = new();
    private readonly List<BatchObservableCollection<PlayingCardViewModel>> _tableauSet = new();
    private readonly CasinoViewModel _casinoViewModel;

    public BatchObservableCollection<PlayingCardViewModel> Foundation1 { get; } = new();

    public BatchObservableCollection<PlayingCardViewModel> Foundation2 { get; } = new();

    public BatchObservableCollection<PlayingCardViewModel> Foundation3 { get; } = new();

    public BatchObservableCollection<PlayingCardViewModel> Foundation4 { get; } = new();

    public BatchObservableCollection<PlayingCardViewModel> Tableau1 { get; } = new();

    public BatchObservableCollection<PlayingCardViewModel> Tableau2 { get; } = new();

    public BatchObservableCollection<PlayingCardViewModel> Tableau3 { get; } = new();

    public BatchObservableCollection<PlayingCardViewModel> Tableau4 { get; } = new();

    public BatchObservableCollection<PlayingCardViewModel> Tableau5 { get; } = new();

    public BatchObservableCollection<PlayingCardViewModel> Tableau6 { get; } = new();

    public BatchObservableCollection<PlayingCardViewModel> Tableau7 { get; } = new();

    public BatchObservableCollection<PlayingCardViewModel> Stock { get; } = new();

    public BatchObservableCollection<PlayingCardViewModel> Waste { get; } = new();


    public ICommand? TurnStockCommand { get; }

}
