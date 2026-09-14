using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Solitaire.Controls;

// One shadow per group prevents darker shadows where cards overlap.
internal sealed class CardCastShadow : Control
{
    private CardShadowShape[] _cards = Array.Empty<CardShadowShape>();
    internal IReadOnlyList<CardShadowShape> Shapes => _cards;
    private double _blur;
    private Rect _drawingBounds;
    private readonly TranslateTransform _translation = new();

    public CardCastShadow()
    {
        IsHitTestVisible = false;
        ClipToBounds = false;
        // Update the shadow bounds without changing the Canvas layout.
        Width = Height = 1;
        RenderTransform = _translation;
    }

    public void UpdateShape(IReadOnlyList<CardShadowShape> cards, Rect bounds, double height, Vector origin)
    {
        var changed = _cards.Length != cards.Count;
        for (var i = 0; !changed && i < cards.Count; i++)
            changed = _cards[i] != cards[i].Translate(-bounds.Position);
        var blur = 1.5 + height * .38;
        if (changed)
        {
            _cards = cards.Select(card => card.Translate(-bounds.Position)).ToArray();
            _drawingBounds = new Rect(bounds.Size);
        }
        if (changed || _blur != blur)
        {
            _blur = blur;
            InvalidateVisual();
        }
        _translation.X = origin.X + bounds.X;
        _translation.Y = origin.Y + bounds.Y;
        Opacity = (.25 - .06 * Math.Min(height / 24, 1)) * Math.Clamp(height / 3, 0, 1);
    }

    public override void Render(DrawingContext context)
    {
        context.Custom(new CardShadowDrawOperation(_cards, _drawingBounds, _blur));
    }
}
