using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Solitaire.Models;
using Solitaire.Utils;

namespace Solitaire.ViewModels.Pages;

public partial class SpiderSolitaireViewModel : CardGameViewModel
{
    public SpiderSolitaireViewModel(CasinoViewModel casinoViewModel) : base(casinoViewModel)
    {
        InitializeTableauSet();

        DealCardsCommand = new RelayCommand(DoDealCards, () => Stock.Count > 0);
        NewGameCommand = new RelayCommand(DoDealNewGame);

        casinoViewModel.SettingsInstance.WhenAnyValue(x => x.Difficulty)
            .Do(x => Difficulty = x)
            .Subscribe();
    }

    /// <inheritdoc />
    protected override void InitializeDeck()
    {
        
        var temp = Enum
            .GetValuesAsUnderlyingType(typeof(CardType))
            .Cast<CardType>()
            .Select(x => Enumerable.Repeat(x, 8))
            .SelectMany(x => x)
            .Select(cardType => new PlayingCardViewModel(this)
                {CardType = cardType, IsFaceDown = true})
            .ToArray();
    
        //  Create a playing card from each card type.
        //  We just keep on adding cards of suits that depend on the
        //  difficulty setting until we have the required 104.
        var playingCards = new List<PlayingCardViewModel>();
        foreach (var card in temp)
        {
            card.IsFaceDown = true;
    
            switch (Difficulty)
            {
                case Difficulty.Easy:
                    //  In easy mode, we have hearts only.
                    if (card.Suit != CardSuit.Hearts)
                        continue;
                    break;
                case Difficulty.Medium:
                    //  In easy mode, we have hearts and spades.
                    if (card.Suit == CardSuit.Diamonds || card.Suit == CardSuit.Clubs)
                        continue;
                    break;
                case Difficulty.Hard:
                    //  In hard mode we have every card.
                    break;
            }
    
            //  Add the card.
            playingCards.Add(card);
    
            //  If we've got 104 we're done.
            if (playingCards.Count >= 104)
                break;
        }
        
        Deck = playingCards.ToImmutableArray();
    }

    private void InitializeTableauSet()
    {
        //  Create the quick access arrays.
        _tableauSet.Add(Tableau1);
        _tableauSet.Add(Tableau2);
        _tableauSet.Add(Tableau3);
        _tableauSet.Add(Tableau4);
        _tableauSet.Add(Tableau5);
        _tableauSet.Add(Tableau6);
        _tableauSet.Add(Tableau7);
        _tableauSet.Add(Tableau8);
        _tableauSet.Add(Tableau9);
        _tableauSet.Add(Tableau10);
    }

    /// <inheritdoc />
    public override string GameName => "Spider Solitaire";

    public override IList<PlayingCardViewModel>? GetCardCollection(PlayingCardViewModel card)
    {
        return Stock.Contains(card) ? Stock : _tableauSet.FirstOrDefault(tableau => tableau.Contains(card));
    }

    private void DoDealNewGame()
    {
        ResetGame();

        var playingCards = GetNewShuffledDeck();
        var tableauBatches = _tableauSet.Select(x => x.DelayNotifications()).ToList();
        using var stockD = Stock.DelayNotifications();

        //  Now distribute them - do the tableau set first.
        for (var i = 0; i < 54; i++)
        {
            var card = playingCards.First();
            playingCards.Remove(card);
            card.IsFaceDown = true;
            tableauBatches[i % 10].Add(card);
        }

        for (var i = 0; i < 10; i++)
        {
            tableauBatches[i].Last().IsFaceDown = false;
            tableauBatches[i].Last().IsPlayable = true;
        }

        //  Finally we add every card that's left over to the stock.
        stockD.AddRange(playingCards);
        playingCards.Clear();

        tableauBatches.ForEach(x => x.Dispose());
        
        //  And we're done.
        StartTimer();
    }

    public override void ResetGame()
    {
        //  Call the base, which stops the timer, clears
        //  the score etc.
        ResetInternalState();

        //  Spider solitaire starts with a score of 500.
        Score = 500;

        //  Clear everything.
        Stock.Clear();
        Foundation.Clear();
        foreach (var tableau in _tableauSet)
            tableau.Clear();
    }

    public ICommand? DealCardsCommand { get; }

    private void DoDealCards()
    {
        //  As a sanity check if the stock is empty we cannot deal cards.
        using var stockD = Stock.DelayNotifications();
         
        var tableauBatches = _tableauSet.Select(x => x.DelayNotifications()).ToList();

        //  If any tableau is empty we cannot deal cards.
        if (tableauBatches.Any(tableau => tableau.Count == 0))
        {
            tableauBatches.ForEach(x=>x.Dispose());
            return;
        }

        foreach (var tableau in tableauBatches)
        {
            if(stockD.Count == 0) break;
            var card = stockD.Last();
            stockD.Remove(card);
            card.IsFaceDown = false;
            card.IsPlayable = true;
            tableau.Add(card);
        }
        
        tableauBatches.ForEach(x=>x.Dispose());

        //  Check each tableau for sequences - then check for victory.
        CheckEachTableau();
        CheckForVictory();
    }

    public override bool CheckAndMoveCard(IList<PlayingCardViewModel> from, IList<PlayingCardViewModel> to,
        PlayingCardViewModel card, bool checkOnly = false)
    {
        //  The trivial case is where from and to are the same.
        if (from.SequenceEqual(to))
            return false;
        
        if (to.SequenceEqual(Stock))
            return false;
        
        //  This is the complicated operation.
        int scoreModifier;

        //  We can move to a tableau only if:
        //  1. It is empty 
        //  2. It is card suit S and we are suit S and Number N-1
        if ((to.Count == 0) ||
            (to.Count > 0 && //to.Last().Suit != card.Suit && 
             (to.Last()).Value == (card.Value + 1)))
        {
            //  Move from tableau to tableau, we always lose a point for this.
            scoreModifier = -1;
        }
        else
            return false;

        //  If we were just checking, we're done.
        if (checkOnly)
            return true;

        //  If we've got here we've passed all tests
        //  and move the card and update the score.
        DoMoveCard(from, to, card);
        Score += scoreModifier;
        Moves++;

        //  Check each tableau for sequences - then check for victory.
        CheckEachTableau();
        CheckForVictory();

        return true;
    }

    private void DoMoveCard(IList<PlayingCardViewModel> from,
        IList<PlayingCardViewModel> to,
        PlayingCardViewModel card)
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
        if (from.Count <= 0) return;
        //  Reveal the top card and make it playable.
        var topCard = from.Last();

        topCard.IsFaceDown = false;
        topCard.IsPlayable = true;
    }

    /// <summary>
    /// Checks each tableau, making sure that only playable cards
    /// can be played and moving any full sequences to the 
    /// foundation.
    /// </summary>
    private void CheckEachTableau()
    {
        //  Go through each tableau.
        foreach (var tableau in _tableauSet)
        {
            //  Go through each card in the tableau from top to bottom.   
            var sequence = true;
            for (int i = tableau.Count - 1, count = 0; i >= 0; i--, count++)
            {
                //  The first card is always face up and playable.
                //  Starting with the first card we are hoping to
                //  find a sequence - only cards in sequence from the top
                //  down will be playable.
                if (count == 0)
                {
                    tableau[i].IsFaceDown = false;
                    tableau[i].IsPlayable = true;
                }
                else if (sequence)
                {
                    //  We are not the top card - do we follow
                    //  in sequence from it - i.e. are we suited and
                    //  with a value one higher?
                    if (tableau[i].Suit == tableau[i + 1].Suit &&
                        tableau[i].Value == tableau[i + 1].Value + 1)
                    {
                        //  We are still in sequence - are we a king?
                        //  If so we've made a full sequence and we
                        //  can remove it.
                        if (tableau[i].Value == 12 && count == 12)
                        {
                            //  Clearing a set gives us an extra 100 points.
                            Score += 100;
                            for (var j = 0; j < 13; j++)
                            {
                                var card = tableau[i];
                                tableau.Remove(card);
                                Foundation.Add(card);
                            }

                            //  Start again.
                            i = tableau.Count;
                            count = -1;
                            continue;
                        }
                    }
                    else
                    {
                        //  We have broken the sequence.
                        sequence = false;
                    }
                }

                //  If the sequence is broken, we're not playable.
                tableau[i].IsPlayable = sequence;
            }
        }
    }

    private void CheckForVictory()
    {
        //  Every card must be in the foundation for the game to be won.
        if (Foundation.Count < 104)
            return;

        //  We've won.
        IsGameWon = true;

        //  Stop the timer.
        StopTimer();

        //  Fire the won event.
        FireGameWonEvent();
    }

    //  For ease of access we have an array of tableau set.
    private readonly List<BatchObservableCollection<PlayingCardViewModel>> _tableauSet = new();

    //  Accessors for the various card stacks.
    public BatchObservableCollection<PlayingCardViewModel> Tableau1 { get; } = new();
    public BatchObservableCollection<PlayingCardViewModel> Tableau2 { get; } = new();
    public BatchObservableCollection<PlayingCardViewModel> Tableau3 { get; } = new();
    public BatchObservableCollection<PlayingCardViewModel> Tableau4 { get; } = new();
    public BatchObservableCollection<PlayingCardViewModel> Tableau5 { get; } = new();
    public BatchObservableCollection<PlayingCardViewModel> Tableau6 { get; } = new();
    public BatchObservableCollection<PlayingCardViewModel> Tableau7 { get; } = new();
    public BatchObservableCollection<PlayingCardViewModel> Tableau8 { get; } = new();
    public BatchObservableCollection<PlayingCardViewModel> Tableau9 { get; } = new();
    public BatchObservableCollection<PlayingCardViewModel> Tableau10 { get; } = new();
    public BatchObservableCollection<PlayingCardViewModel> Stock { get; } = new();
    public BatchObservableCollection<PlayingCardViewModel> Foundation { get; } = new();


    [ObservableProperty] private Difficulty _difficulty;
}