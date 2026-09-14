using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
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
        NewGameCommand = new AsyncRelayCommand(DoDealNewGame);

        casinoViewModel.SettingsInstance.ObserveProperty(nameof(SettingsViewModel.Difficulty), static x => x.Difficulty)
            .Do(x => Difficulty = x)
            .Subscribe();
    }

    /// <inheritdoc />
    protected override void InitializeDeck()
    {

        var temp = Enum
            .GetValuesAsUnderlyingType<CardType>()
            .Cast<CardType>()
            .Select(x => Enumerable.Repeat(x, 8))
            .SelectMany(x => x)
            .Select(cardType => new PlayingCardViewModel(this)
            { CardType = cardType, IsFaceDown = true })
            .ToArray();

        var playingCards = new List<PlayingCardViewModel>();
        foreach (var card in temp)
        {
            card.IsFaceDown = true;

            switch (Difficulty)
            {
                case Difficulty.Easy:
                    if (card.Suit != CardSuit.Hearts)
                        continue;
                    break;
                case Difficulty.Medium:
                    if (card.Suit == CardSuit.Diamonds || card.Suit == CardSuit.Clubs)
                        continue;
                    break;
                case Difficulty.Hard:
                    break;
            }

            playingCards.Add(card);

            if (playingCards.Count >= 104)
                break;
        }

        Deck = playingCards.ToImmutableArray();
    }

    private void InitializeTableauSet()
    {
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
    public override string GameName => "Spider";

    public override IList<PlayingCardViewModel>? GetCardCollection(PlayingCardViewModel card)
    {
        return Stock.Contains(card) ? Stock : _tableauSet.FirstOrDefault(tableau => tableau.Contains(card));
    }

    private async Task DoDealNewGame()
    {
        ResetGame();
        var cancellation = ActionCancellation;

        var playingCards = GetNewShuffledDeck();


        using (var stock0 = Foundation.DelayNotifications())
        {
            stock0.AddRange(playingCards);
        }

        if (!await PauseGameAction(1000, cancellation))
            return;

        using (var stock0 = Foundation.DelayNotifications())
        {
            stock0.Clear();
        }

        for (var i = 0; i < 54; i++)
        {
            var card = playingCards.First();
            playingCards.Remove(card);
            card.IsFaceDown = true;
            _tableauSet[i % 10].Add(card);

            if (!await PauseGameAction(50, cancellation))
                return;
        }

        for (var i = 0; i < 10; i++)
        {
            _tableauSet[i].Last().IsFaceDown = false;
            _tableauSet[i].Last().IsPlayable = true;
        }

        foreach (var card in playingCards)
        {
            Stock.Add(card);
            if (!await PauseGameAction(50, cancellation))
                return;
        }

        playingCards.Clear();

        StartTimer();
    }

    public override void ResetGame()
    {
        ResetInternalState();

        Score = 500;

        Stock.Clear();
        Foundation.Clear();
        foreach (var tableau in _tableauSet)
            tableau.Clear();
    }

    public ICommand? DealCardsCommand { get; }

    private void DoDealCards()
    {
        using var stockD = Stock.DelayNotifications();

        var tableauBatches = _tableauSet.Select(x => x.DelayNotifications()).ToList();

        if (tableauBatches.Any(tableau => tableau.Count == 0))
        {
            tableauBatches.ForEach(x => x.Dispose());
            return;
        }

        foreach (var tableau in tableauBatches)
        {
            if (stockD.Count == 0)
                break;
            var card = stockD.Last();
            stockD.Remove(card);
            card.IsFaceDown = false;
            card.IsPlayable = true;
            tableau.Add(card);
        }

        tableauBatches.ForEach(x => x.Dispose());

        CheckEachTableau();
        CheckForVictory();
    }

    public override bool CheckAndMoveCard(IList<PlayingCardViewModel> from, IList<PlayingCardViewModel> destination,
        PlayingCardViewModel card, bool checkOnly = false)
    {
        if (from.SequenceEqual(destination))
            return false;

        if (destination.SequenceEqual(Stock))
            return false;

        int scoreModifier;

        if ((destination.Count == 0) ||
            (destination.Count > 0 && //destination.Last().Suit != card.Suit &&
             (destination.Last()).Value == (card.Value + 1)))
        {
            scoreModifier = -1;
        }
        else
            return false;

        if (checkOnly)
            return true;

        DoMoveCard(from, destination, card, scoreModifier);
        Score += scoreModifier;
        Moves++;

        CheckEachTableau();
        CheckForVictory();

        return true;
    }

    private void DoMoveCard(IList<PlayingCardViewModel> from,
        IList<PlayingCardViewModel> to,
        PlayingCardViewModel card, int scoreModifier)
    {
        var run = new List<PlayingCardViewModel>();
        for (var i = from.IndexOf(card); i < from.Count; i++)
            run.Add(from[i]);

        foreach (var runCard in run)
            from.Remove(runCard);
        foreach (var runCard in run)
            to.Add(runCard);

        if (from.Count <= 0)
            return;
        var topCard = from.Last();

        var wasFaceDown = topCard.IsFaceDown;
        var wasPlayable = topCard.IsPlayable;
        topCard.IsFaceDown = false;
        topCard.IsPlayable = true;

        RecordMoves(new MoveOperation(from, to, run, scoreModifier), new GenericOperation(() =>
        {
            topCard.IsFaceDown = wasFaceDown;
            topCard.IsPlayable = wasPlayable;
        }));
    }

    /// <summary>
    /// Marks playable cards and moves complete suit sequences to the foundation.
    /// </summary>
    private void CheckEachTableau()
    {
        foreach (var tableau in _tableauSet)
        {
            var sequence = true;
            for (int i = tableau.Count - 1, count = 0; i >= 0; i--, count++)
            {
                if (count == 0)
                {
                    tableau[i].IsFaceDown = false;
                    tableau[i].IsPlayable = true;
                }
                else if (sequence)
                {
                    if (tableau[i].Suit == tableau[i + 1].Suit &&
                        tableau[i].Value == tableau[i + 1].Value + 1)
                    {
                        // A complete suit sequence moves to the foundation.
                        if (tableau[i].Value == 12 && count == 12)
                        {
                            Score += 100;
                            for (var j = 0; j < 13; j++)
                            {
                                var card = tableau[i];
                                tableau.Remove(card);
                                Foundation.Add(card);
                            }

                            i = tableau.Count;
                            count = -1;
                            continue;
                        }
                    }
                    else
                    {
                        sequence = false;
                    }
                }

                tableau[i].IsPlayable = sequence;
            }
        }
    }

    private void CheckForVictory()
    {
        if (Foundation.Count < 104)
            return;

        IsGameWon = true;

        StopTimer();

        FireGameWonEvent();
    }

    private readonly List<BatchObservableCollection<PlayingCardViewModel>> _tableauSet = new();

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
