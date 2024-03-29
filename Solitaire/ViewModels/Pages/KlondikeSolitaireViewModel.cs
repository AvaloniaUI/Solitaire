﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Solitaire.Models;
using Solitaire.Utils;

namespace Solitaire.ViewModels.Pages;

/// <summary>
/// The Klondike Solitaire View Model.
/// </summary>
public partial class KlondikeSolitaireViewModel : CardGameViewModel
{
    /// <inheritdoc />
    public override string GameName => "Klondike";

    [ObservableProperty] private DrawMode _drawMode;
    private bool _isTurning;
 
    public KlondikeSolitaireViewModel(CasinoViewModel casinoViewModel) : base(casinoViewModel)
    {
        _casinoViewModel = casinoViewModel;
        InitializeFoundationsAndTableauSet();

        //  Create the turn stock command.
        TurnStockCommand = new AsyncRelayCommand(DoTurnStock);
        AutoMoveCommand = new AsyncRelayCommand(TryMoveAllCardsToAppropriateFoundations);
        NewGameCommand = new AsyncRelayCommand(DoDealNewGame);
    }

    private void InitializeFoundationsAndTableauSet()
    {
        //  Create the quick access arrays.
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

    /// <summary>
    /// Gets the card collection for the specified card.
    /// </summary>
    /// <param name="card">The card.</param>
    /// <returns></returns>
    public override IList<PlayingCardViewModel>? GetCardCollection(PlayingCardViewModel card)
    {
        if (Stock.Contains(card)) return Stock;
        if (Waste.Contains(card)) return Waste;

        foreach (var foundation in _foundations.Where(foundation => foundation.Contains(card)))
            return foundation;

        return _tableauSet.FirstOrDefault(tableau => tableau.Contains(card));
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
    /// Deals a new game.
    /// </summary>
    private async Task DoDealNewGame()
    {
        DrawMode = _casinoViewModel.SettingsInstance.DrawMode;

        ResetGame();

        var playingCards = GetNewShuffledDeck();
        
        using (var stock0 = Stock.DelayNotifications())
        {
            stock0.AddRange(playingCards);
        }
        
        await Task.Delay(600);
        
        using (var stock0 = Stock.DelayNotifications())
        {
            stock0.Clear();
        }
        
        //  Now distribute them - do the tableau sets first.
        for (var i = 0; i < 7; i++)
        {
            var tempTableau = new List<PlayingCardViewModel>();

            //  We have i face down cards and 1 face up card.
            for (var j = 0; j < i; j++)
            {
                var faceDownCardViewModel = playingCards.First();
                playingCards.Remove(faceDownCardViewModel);
                faceDownCardViewModel.IsFaceDown = true;
                tempTableau.Add(faceDownCardViewModel);
            }

            //  Add the face up card.
            var faceUpCardViewModel = playingCards.First();
            playingCards.Remove(faceUpCardViewModel);
            faceUpCardViewModel.IsFaceDown = false;
            faceUpCardViewModel.IsPlayable = true;
            tempTableau.Add(faceUpCardViewModel);


            foreach (var card in tempTableau)
            {
                _tableauSet[i].Add(card);

                await Task.Delay(75);
            }
        }

        //  Finally we add every card that's left over to the stock.
        foreach (var playingCard in playingCards)
        {
            playingCard.IsFaceDown = true;
            playingCard.IsPlayable = false;
        }

        using var stockD = Stock.DelayNotifications();
        
        stockD.AddRange(playingCards);


        //  And we're done.
        StartTimer();
    }

    public override void ResetGame()
    {
        using var stockD = Stock.DelayNotifications();
        using var wasteD = Waste.DelayNotifications();

        DrawMode = _casinoViewModel.SettingsInstance.DrawMode;

        //  Call the base, which stops the timer, clears
        //  the score etc.
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

        //  Clear everything.
        stockD.Clear();
        wasteD.Clear();
    }

    /// <summary>
    /// Turns cards from the stock into the waste.
    /// </summary>
    private async Task DoTurnStock()
    {
        if(_isTurning)
            return;

        _isTurning = true;
        
        //  If the stock is empty, put every card from the waste back into the stock.
        if (Stock.Count == 0)
        {
            foreach (var card in Waste)
            {
                card.IsFaceDown = true;
                card.IsPlayable = false;
                Stock.Insert(0, card);

                await Task.Delay(175);
            }
            Waste.Clear();
        }
        else
        { 
            //  Work out how many cards to draw.
            var cardsToDraw = DrawMode switch
            {
                DrawMode.DrawOne => 1,
                DrawMode.DrawThree => 3,
                _ => 1
            };

            //  Put up to three cards in the waste.
            for (var i = 0; i < cardsToDraw; i++)
            {
                if (Stock.Count <= 0) continue;
                var card = Stock.Last();
                Stock.Remove(card);
                card.IsFaceDown = false;
                card.IsPlayable = false;
                Waste.Add(card);

                await Task.Delay(175);
            }
        }

        //  Everything in the waste must be not playable,
        //  apart from the top card.
        foreach (var wasteCard in Waste)
            wasteCard.IsPlayable = wasteCard == Waste.Last();

        _isTurning = false;
    }
    /// <summary>
    /// Tries the move all cards to appropriate foundations.
    /// </summary>
    private async Task TryMoveAllCardsToAppropriateFoundations()
    {
        //  Go through the top card in each tableau - keeping
        //  track of whether we moved one.
        var keepTrying = true;
        
        while (keepTrying)
        {
            var movedACard = false;
            
            if (Waste.Count > 0)
            {
                if (TryMoveCardToAppropriateFoundation(Waste.Last()))
                {
                    movedACard = true;
                    await Task.Delay(75);
                }
            }

            foreach (var tableau in _tableauSet)
            {
                if (tableau.Count > 0)
                {
                    if (TryMoveCardToAppropriateFoundation(tableau.Last()))
                    {
                        movedACard = true;
                        await Task.Delay(75);
                    }
                }
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
        //  Try the top of the waste first.
        if (Waste.LastOrDefault() == card)
            foreach (var foundation in _foundations)
                if (CheckAndMoveCard(Waste, foundation, card))
                    return true;

        //  Is the card in a tableau?
        var inTableau = false;
        var i = 0;
        for (; i < _tableauSet.Count && inTableau == false; i++)
            inTableau = _tableauSet[i].Contains(card);

        //  It's if its not in a tableau and it's not the top
        //  of the waste, we cannot move it.
        if (inTableau == false)
            return false;

        //  Try and move to each foundation.
        foreach (var foundation in _foundations)
            if (CheckAndMoveCard(_tableauSet[i - 1], foundation, card))
                return true;

        //  We couldn't move the card.
        return false;
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

        //  This is the complicated operation.
        int scoreModifier;

        //  Are we moving from the waste?
        if (from.SequenceEqual(Waste))
        {
            //  Are we moving to a foundation?
            if (_foundations.Contains(to))
            {
                //  We can move to a foundation only if:
                //  1. It is empty and we are an ace.
                //  2. It is card SN and we are suit S and Number N+1
                 if (GetSuitForFoundations(to) == card.Suit && ((to.Count == 0 && card.Value == 0) ||
                    (to.Count > 0 && to.Last().Suit == card.Suit && to.Last().Value == card.Value - 1)))
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
                if ((to.Count == 0 && card.Value == 12) ||
                    (to.Count > 0 && to.Last().Colour != card.Colour && to.Last().Value == card.Value + 1))
                {
                    //  Move from waste to tableau.
                    scoreModifier = 5;
                }
                else
                    return false;
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
                if (GetSuitForFoundations(to) == card.Suit && ((to.Count == 0 && card.Value == 0) ||
                    (to.Count > 0 && to.Last().Suit == card.Suit && to.Last().Value == card.Value - 1)))
                {
                    //  Move from tableau to foundation.
                    scoreModifier = 10;
                }
                else
                    return false;
            }
            //  Are we moving to another tableau?
            else if (_tableauSet.Contains(to))
            {
                //  We can move to a tableau only if:
                //  1. It is empty and we are a king.
                //  2. It is card CN and we are color !C and Number N-1
                if ((to.Count == 0 && card.Value == 12) ||
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
        //  Are we moving from a foundation?
        else if (_foundations.Contains(from))
        {
            //  Are we moving to a tableau?
            if (_tableauSet.Contains(to))
            {
                //  We can move to a tableau only if:
                //  1. It is empty and we are a king.
                //  2. It is card CN and we are color !C and Number N-1
                if ((to.Count == 0 && card.Value == 12) ||
                    (to.Count > 0 && to.Last().Colour != card.Colour && to.Last().Value == card.Value + 1))
                {
                    //  Move from foundation to tableau.
                    scoreModifier = -15;
                }
                else
                    return false;
            }
            //  Are we moving to another foundation?
            else if (_foundations.Contains(to))
            {
                if (GetSuitForFoundations(to) != card.Suit && card.Value == 0)
                {
                    return false;
                }
                
                //  We can move from a foundation to a foundation only 
                //  if the source foundation has one card (the ace) and the
                //  destination foundation has no cards).
                if (from.Count == 1 && to.Count == 0)
                {
                    //  The move is valid, but has no impact on the score.
                    scoreModifier = 0;
                }
                else
                    return false;
            }
            //  Any other move from a foundation is wrong.
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

        //  If we have moved from the waste, we must 
        //  make sure that the top of the waste is playable.
        if (from.SequenceEqual(Waste) && Waste.Count > 0)
            Waste.Last().IsPlayable = true;

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

        //  Are there any cards left in the from pile?
        if (from.Count > 0)
        {
            //  Reveal the top card and make it playable.
            var topCardViewModel = from.Last();

            topCardViewModel.IsFaceDown = false;
            topCardViewModel.IsPlayable = true;

            RecordMoves(new MoveOperation(from, to, run, scoreModifier), new GenericOperation(() =>
            {
                topCardViewModel.IsFaceDown = true;
                topCardViewModel.IsPlayable = false;
            }));
        }
        else
        {
            RecordMoves(new MoveOperation(from, to, run, scoreModifier));
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


    /// <summary>
    /// The turn stock command.
    /// </summary> 
    public ICommand? TurnStockCommand { get; }

}