using System.Collections.Generic;
using Avalonia;

namespace Solitaire.Controls;

// Cards with the same pose need only the top artwork, even during a deal.
internal sealed class CardStackVisibility
{
    private readonly Dictionary<(CardPose Pose, Size Size, bool Floating), PlayingCard> _tops = new();

    public void Update(IReadOnlyCollection<PlayingCard> cards)
    {
        foreach (var card in cards)
        {
            var key = (card.Pose, card.Bounds.Size, card.IsFloating);
            // For equal ZIndex values, the last child is on top.
            if (!_tops.TryGetValue(key, out var top) || card.ZIndex >= top.ZIndex)
                _tops[key] = card;
        }
        foreach (var card in cards)
        {
            var exposed = ReferenceEquals(_tops[(card.Pose, card.Bounds.Size, card.IsFloating)], card);
            // Opacity hides artwork without changing layout or bindings.
            // A pose change reveals the next card in the same update.
            card.SetCurrentValue(Visual.OpacityProperty, exposed ? 1d : 0d);
        }
        _tops.Clear();
    }
}
