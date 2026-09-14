using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using Solitaire.Models;
using Solitaire.Utils;

namespace Solitaire.ViewModels.Pages;

public partial class FreeCellSolitaireViewModel : CardGameViewModel
{
    /// <inheritdoc />
    public override string GameName => "FreeCell";

    [ObservableProperty] private DrawMode _drawMode;
    [ObservableProperty] private List<PlayingCardViewModel>? _playingCards;

    public FreeCellSolitaireViewModel(CasinoViewModel casinoViewModel) : base(casinoViewModel)
    {
        InitializeFoundationsAndTableauSet();

        AutoMoveCommand = new AsyncRelayCommand(TryMoveAllCardsToAppropriateFoundations);

        NewGameCommand = new AsyncRelayCommand(DoDealNewGame);

        casinoViewModel.SettingsInstance.ObserveProperty(nameof(SettingsViewModel.DrawMode), static x => x.DrawMode)
            .Subscribe(x => DrawMode = x);
    }

    private void InitializeFoundationsAndTableauSet()
    {
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

    public override IList<PlayingCardViewModel>? GetCardCollection(PlayingCardViewModel card)
    {
        if (Cell1.Contains(card))
            return Cell1;
        if (Cell2.Contains(card))
            return Cell2;
        if (Cell3.Contains(card))
            return Cell3;
        if (Cell4.Contains(card))
            return Cell4;

        foreach (var foundation in _foundations.Where(foundation => foundation.Contains(card)))
            return foundation;

        return _tableauSet.FirstOrDefault(tableau => tableau.Contains(card));
    }

    private async Task DoDealNewGame()
    {
        ResetGame();
        var cancellation = ActionCancellation;

        var playingCards = GetNewShuffledDeck();

        using (var stock0 = Cell1.DelayNotifications())
        {
            stock0.AddRange(playingCards);
        }

        if (!await PauseGameAction(600, cancellation))
            return;

        using (var stock0 = Cell1.DelayNotifications())
        {
            stock0.Clear();
        }

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

                _tableauSet[i].Add(faceUpCardViewModel);

                playingCards.Remove(faceUpCardViewModel);

                if (!await PauseGameAction(75, cancellation))
                    return;
            }
        }

        StartTimer();
    }

    public override void ResetGame()
    {
        ResetInternalState();


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
    /// Moves eligible cards to their foundations.
    /// </summary>
    private async Task TryMoveAllCardsToAppropriateFoundations()
    {
        var cancellation = ActionCancellation;
        if (Cell1.Count > 0 && TryMoveCardToAppropriateFoundation(Cell1.Last()))
        {
            if (!await PauseGameAction(75, cancellation))
                return;
        }

        if (Cell2.Count > 0 && TryMoveCardToAppropriateFoundation(Cell2.Last()))
        {
            if (!await PauseGameAction(75, cancellation))
                return;
        }

        if (Cell3.Count > 0 && TryMoveCardToAppropriateFoundation(Cell3.Last()))
        {
            if (!await PauseGameAction(75, cancellation))
                return;
        }

        if (Cell4.Count > 0 && TryMoveCardToAppropriateFoundation(Cell4.Last()))
        {
            if (!await PauseGameAction(75, cancellation))
                return;
        }

        var keepTrying = true;

        while (keepTrying)
        {
            var movedACard = false;

            movedACard |= await MoveTableauCardsToFoundations(_tableauSet, TryMoveCardToAppropriateFoundation, cancellation);

            keepTrying = movedACard;
        }
    }

    /// <summary>
    /// Moves an eligible card to a foundation.
    /// </summary>
    /// <param name="card">The card.</param>
    /// <returns>True after a successful move.</returns>
    private bool TryMoveCardToAppropriateFoundation(PlayingCardViewModel card)
    {
        var tableauPlusCells = _tableauSet.Concat(_cells).ToList();

        var movable = false;
        var i = 0;
        for (; i < tableauPlusCells.Count && movable == false; i++)
            movable = tableauPlusCells[i].Contains(card);

        if (!movable)
            return false;

        foreach (var foundation in _foundations)
            if (CheckAndMoveCard(tableauPlusCells[i - 1], foundation, card))
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
        if (from.SequenceEqual(destination))
            return false;

        var freeCells = _cells.Count(x => x.Count == 0);

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

        if (!TryGetMoveScore(from, destination, card, out var scoreModifier))
            return false;

        if (checkOnly)
            return true;

        MoveCard(from, destination, card, scoreModifier);
        Score += scoreModifier;
        Moves++;

        CheckForVictory();

        return true;
    }

    private bool TryGetMoveScore(IList<PlayingCardViewModel> from,
        IList<PlayingCardViewModel> destination, PlayingCardViewModel card, out int score)
    {
        score = 0;
        if (!_cells.Contains(from) && !_tableauSet.Contains(from))
            return false;
        if (_foundations.Contains(destination))
        {
            score = 10;
            return CardMoveRules.CanBuildFoundation(destination, card, _foundations, false);
        }
        if (_tableauSet.Contains(destination))
            return CardMoveRules.CanPlaceOnTableau(destination, card, true);
        return _cells.Contains(destination) && destination.Count == 0 && from.Count - from.IndexOf(card) <= 1;
    }

    private void MoveCard(IList<PlayingCardViewModel> from,
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

        RecordMoves(new MoveOperation(from, to, run, scoreModifier));

        if (from.Count > 0)
        {
            var topCardViewModel = from.Last();

            topCardViewModel.IsFaceDown = false;
            topCardViewModel.IsPlayable = true;
        }
    }

    private void CheckForVictory()
    {
        foreach (var foundation in _foundations)
            if (foundation.Count < 13)
                return;

        IsGameWon = true;

        StopTimer();

        FireGameWonEvent();
    }

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

}
