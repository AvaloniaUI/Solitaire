using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Solitaire.Models;
using Solitaire.Utils;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Solitaire.ViewModels.Pages;

/// <summary>
/// The Klondike Solitaire View Model.
/// </summary>
public partial class FreeCellSolitaireViewModel : CardGameViewModel
{
    /// <inheritdoc />
    public override string GameName => "FreeCell Solitaire";

    [ObservableProperty] private DrawMode _drawMode;
    [ObservableProperty] private List<PlayingCardViewModel> _playingCards;

    public FreeCellSolitaireViewModel(CasinoViewModel casinoViewModel) : base(casinoViewModel)
    {
        InitializeFoundationsAndTableauSet();

        AppropriateFoundationsCommand = new RelayCommand(TryMoveAllCardsToAppropriateFoundations);

        NewGameCommand = new AsyncRelayCommand(DoDealNewGame);

        casinoViewModel.SettingsInstance.WhenAnyValue(x => x.DrawMode)
            .Subscribe(x => DrawMode = x);
    }

    private void InitializeFoundationsAndTableauSet()
    {
        //  Create the quick access arrays.
        _cells.Add(Cell1);
        _cells.Add(Cell2);
        _cells.Add(Cell3);
        _cells.Add(Cell4);

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
        _tableauSet.Add(Tableau8);
    }

    /// <summary>
    /// Gets the card collection for the specified card.
    /// </summary>
    /// <param name="card">The card.</param>
    /// <returns></returns>
    public override IList<PlayingCardViewModel>? GetCardCollection(PlayingCardViewModel card)
    {
        if (Cell1.Contains(card)) return Cell1;
        if (Cell2.Contains(card)) return Cell2;
        if (Cell3.Contains(card)) return Cell3;
        if (Cell4.Contains(card)) return Cell4;

        foreach (var foundation in _foundations.Where(foundation => foundation.Contains(card)))
            return foundation;

        return _tableauSet.FirstOrDefault(tableau => tableau.Contains(card));
    }

    /// <summary>
    /// Deals a new game.
    /// </summary>
    private async Task DoDealNewGame()
    {
        ResetGame();

        var playingCards = GetNewShuffledDeck();
        
        using (var stock0 = Cell1.DelayNotifications())
        {
            stock0.AddRange(playingCards);
        }
        
        await Task.Delay(600);
        
        using (var stock0 = Cell1.DelayNotifications())
        {
            stock0.Clear();
        }

        var tableauBatches = _tableauSet.Select(x => x.DelayNotifications()).ToList();

        //  Now distribute them - do the tableau sets first.
        while (playingCards.Count > 0)
        {
            for (var i = 0; i < 8; i++)
            {
                if (playingCards.Count == 0)
                {
                    break;
                }

                var faceUpCardViewModel = playingCards.First();

                faceUpCardViewModel.IsFaceDown = false;
                faceUpCardViewModel.IsPlayable = true;

                tableauBatches[i].Add(faceUpCardViewModel);

                playingCards.Remove(faceUpCardViewModel);
            }
        }

        tableauBatches.ForEach(x => x.Dispose());

        //  And we're done.
        StartTimer();
    }

    public override void ResetGame()
    {
        //  Call the base, which stops the timer, clears
        //  the score etc.
        ResetInternalState();

        //  Clear everything.

        Cell1.Clear();
        Cell2.Clear();
        Cell3.Clear();
        Cell4.Clear();

        foreach (var tableau in _tableauSet)
            tableau.Clear();
        foreach (var foundation in _foundations)
            foundation.Clear();
    }

    /// <summary>
    /// Tries the move all cards to appropriate foundations.
    /// </summary>
    private void TryMoveAllCardsToAppropriateFoundations()
    {
        //  Go through the top card in each tableau - keeping
        //  track of whether we moved one.
        if (Cell1.Count > 0)
            TryMoveCardToAppropriateFoundation(Cell1.Last());
        
        if (Cell2.Count > 0)
            TryMoveCardToAppropriateFoundation(Cell2.Last());
        
        if (Cell3.Count > 0)
            TryMoveCardToAppropriateFoundation(Cell3.Last());
        
        if (Cell4.Count > 0)
            TryMoveCardToAppropriateFoundation(Cell4.Last());

        var keepTrying = true;
        while (keepTrying)
        {
            var movedACard = false;


            foreach (var tableau in _tableauSet)
            {
                if (tableau.Count > 0)
                    if (TryMoveCardToAppropriateFoundation(tableau.Last()))
                        movedACard = true;
            }

            //  We'll keep trying if we moved a card.
            keepTrying = movedACard;
        }
    }

    /// <summary>
    /// Tries the move the card to its appropriate foundation.
    /// </summary>
    /// <param name="card">The card.</param>
    /// <returns>True if card moved.</returns>
    private bool TryMoveCardToAppropriateFoundation(PlayingCardViewModel card)
    {
        var _tableauPlusCells = _tableauSet.Concat(_cells).ToList();

        //  Is the card in a tableau?
        var movable = false;
        var i = 0;
        for (; i < _tableauPlusCells.Count && movable == false; i++)
            movable = _tableauPlusCells[i].Contains(card);

        //  It's if its not in a tableau and it's not the top
        //  of the waste, we cannot move it.
        if (!movable)
            return false;

        //  Try and move to each foundation.
        foreach (var foundation in _foundations)
            if (CheckAndMoveCard(_tableauPlusCells[i - 1], foundation, card))
                return true;

        //  We couldn't move the card.
        return false;
    }

    private CardSuit GetSuitForFoundations(IList<PlayingCardViewModel> cell)
    {
        if (ReferenceEquals(cell, _foundations[0]))
            return CardSuit.Hearts;

        if (ReferenceEquals(cell, _foundations[1]))
            return CardSuit.Clubs;

        if (ReferenceEquals(cell, _foundations[2]))
            return CardSuit.Diamonds;

        if (ReferenceEquals(cell, _foundations[3]))
            return CardSuit.Spades;

        throw new InvalidConstraintException();
    }

    /// <summary>
    /// Moves the card.
    /// </summary>
    /// <param name="from">The set we're moving from.</param>
    /// <param name="to">The set we're moving to.</param>
    /// <param name="card">The card we're moving.</param>
    /// <param name="checkOnly">if set to <c>true</c> we only check if we CAN move, but don't actually move.</param>
    /// <returns>True if a card was moved.</returns>
    public override bool CheckAndMoveCard(IList<PlayingCardViewModel> from,
        IList<PlayingCardViewModel> to,
        PlayingCardViewModel card,
        bool checkOnly = false)
    {
        //  The trivial case is where from and to are the same.
        if (from.SequenceEqual(to))
            return false;

        var freeCells = _cells.Count(x => x.Count == 0);

        //  Identify the run of cards we're moving.
        var run = new List<PlayingCardViewModel>();
        for (var i = from.IndexOf(card); i < from.Count; i++)
            run.Add(from[i]);

        if (run.Count > freeCells + 1)
            return false;

        if (run.Count > 1)
        {
            for (var i = 0; i < run.Count - 1; i++)
            {
                if (run[i].Value - 1 != run[i + 1].Value)
                {
                    return false;
                }
            }
        }

        //  This is the complicated operation.
        int scoreModifier;

        //  Are we moving from the cells?
        if (_cells.Contains(from))
        {
            //  Are we moving to a foundation?
            if (_foundations.Contains(to))
            {
                //  We can move to a foundation only if:
                //  1. It is empty and we are an ace.
                //  2. It is card SN and we are suit S and Number N+1
                if (GetSuitForFoundations(to) == card.Suit && 
                    ((to.Count == 0 && card.Value == 0) || (to.Count > 0 && to.Last().Value == card.Value - 1)))
                {
                    //  Move from waste to foundation.
                    scoreModifier = 10;
                }
                else
                    return false;
            }
            //  Are we moving to a tableau?
            else if (_tableauSet.Contains(to))
            {
                //  We can move to a tableau only if:
                //  1. It is empty and we are a king.
                //  2. It is card CN and we are color !C and Number N-1
                if (to.Count == 0 ||
                    (to.Count > 0 && to.Last().Colour != card.Colour && to.Last().Value == card.Value + 1))
                {
                    scoreModifier = 0;
                }
                else
                    return false;
            }
            else if (_cells.Contains(to))
            {
                if (to.Count > 0 || from.Count - from.IndexOf(card) > 1)
                {
                    return false;
                }

                scoreModifier = 0;
            }
            //  Any other move from the waste is wrong.
            else
                return false;
        }
        //  Are we moving from a tableau?
        else if (_tableauSet.Contains(from))
        {
            //  Are we moving to a foundation?
            if (_foundations.Contains(to))
            {
                //  We can move to a foundation only if:
                //  1. It is empty and we are an ace.
                //  2. It is card SN and we are suit S and Number N+1
                if (GetSuitForFoundations(to) == card.Suit && 
                    ((to.Count == 0 && card.Value == 0) || (to.Count > 0 && to.Last().Value == card.Value - 1)))
                {
                    //  Move from tableau to foundation.
                    scoreModifier = 10;
                }
                else
                    return false;
            }
            else if (_cells.Contains(to))
            {
                if (to.Count > 0 || from.Count - from.IndexOf(card) > 1)
                {
                    return false;
                }

                scoreModifier = 0;
            }
            //  Are we moving to another tableau?
            else if (_tableauSet.Contains(to))
            {
                //  We can move to a tableau only if:
                //  1. It is empty and we are a king.
                //  2. It is card CN and we are color !C and Number N-1
                if ((to.Count == 0) ||
                    (to.Count > 0 && to.Last().Colour != card.Colour && to.Last().Value == card.Value + 1))
                {
                    //  Move from tableau to tableau.
                    scoreModifier = 0;
                }
                else
                    return false;
            }
            //  Any other move from a tableau is wrong.
            else
                return false;
        }
        else
            return false;

        //  If we were just checking, we're done.
        if (checkOnly)
            return true;

        //  If we've got here we've passed all tests
        //  and move the card and update the score.
        MoveCard(from, to, card, scoreModifier);
        Score += scoreModifier;
        Moves++;

        //  Check for victory.
        CheckForVictory();

        return true;
    }

    /// <summary>
    /// Actually moves the card.
    /// </summary>
    /// <param name="from">The stack to move from.</param>
    /// <param name="to">The stack to move to.</param>
    /// <param name="card">The card.</param>
    private void MoveCard(IList<PlayingCardViewModel> from,
        IList<PlayingCardViewModel> to,
        PlayingCardViewModel card, int scoreModifier)
    {
        //  Identify the run of cards we're moving.
        var run = new List<PlayingCardViewModel>();
        for (var i = from.IndexOf(card); i < from.Count; i++)
            run.Add(from[i]);

        //  This function will move the card, as well as setting the 
        //  playable properties of the cards revealed.
        foreach (var runCard in run)
            from.Remove(runCard);
        foreach (var runCard in run)
            to.Add(runCard);

        RecordMove(from, to, run, scoreModifier);

        //  Are there any cards left in the from pile?
        if (from.Count > 0)
        {
            //  Reveal the top card and make it playable.
            var topCardViewModel = from.Last();

            topCardViewModel.IsFaceDown = false;
            topCardViewModel.IsPlayable = true;
        }
    }

    /// <summary>
    /// Checks for victory.
    /// </summary>
    private void CheckForVictory()
    {
        //  We've won if every foundation is full.
        foreach (var foundation in _foundations)
            if (foundation.Count < 13)
                return;

        //  We've won.
        IsGameWon = true;

        //  Stop the timer.
        StopTimer();

        //  Fire the won event.
        FireGameWonEvent();
    }

    //  For ease of access we have arrays of the foundations and tableau set.
    private readonly List<BatchObservableCollection<PlayingCardViewModel>> _cells = new();
    private readonly List<BatchObservableCollection<PlayingCardViewModel>> _foundations = new();
    private readonly List<BatchObservableCollection<PlayingCardViewModel>> _tableauSet = new();

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

    public BatchObservableCollection<PlayingCardViewModel> Tableau8 { get; } = new();

    public BatchObservableCollection<PlayingCardViewModel> Cell1 { get; } = new();

    public BatchObservableCollection<PlayingCardViewModel> Cell2 { get; } = new();

    public BatchObservableCollection<PlayingCardViewModel> Cell3 { get; } = new();

    public BatchObservableCollection<PlayingCardViewModel> Cell4 { get; } = new();

    public ICommand? AppropriateFoundationsCommand { get; }
}