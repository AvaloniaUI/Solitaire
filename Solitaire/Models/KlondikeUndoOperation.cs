using System.Collections.Generic;
using System.Linq;
using Solitaire.Utils;
using Solitaire.ViewModels;

namespace Solitaire.Models;

// Preserve both piles and their card states for undo.
internal sealed class KlondikeUndoOperation : CardGameViewModel.CardOperation
{
    private readonly PileState _source;
    private readonly PileState _destination;
    private readonly int _score;
    private readonly int _moves;

    public KlondikeUndoOperation(CardGameViewModel game, IList<PlayingCardViewModel> source,
        IList<PlayingCardViewModel> destination)
    {
        _source = new PileState(source);
        _destination = new PileState(destination);
        _score = game.Score;
        _moves = game.Moves;
    }

    public override void Revert(CardGameViewModel game)
    {
        _source.Pile.Clear();
        _destination.Pile.Clear();
        _source.Restore();
        _destination.Restore();
        game.Score = _score;
        game.Moves = _moves;
    }

    private sealed class PileState
    {
        public IList<PlayingCardViewModel> Pile { get; }
        private readonly CardState[] _cards;

        public PileState(IList<PlayingCardViewModel> pile)
        {
            Pile = pile;
            _cards = pile.Select(card => new CardState(card, card.IsFaceDown, card.IsPlayable)).ToArray();
        }

        public void Restore()
        {
            using var batch = (Pile as BatchObservableCollection<PlayingCardViewModel>)?.DelayNotifications();
            foreach (var saved in _cards)
            {
                saved.Card.IsFaceDown = saved.IsFaceDown;
                saved.Card.IsPlayable = saved.IsPlayable;
                (batch ?? Pile).Add(saved.Card);
            }
        }
    }

    private readonly record struct CardState(PlayingCardViewModel Card, bool IsFaceDown, bool IsPlayable);
}
