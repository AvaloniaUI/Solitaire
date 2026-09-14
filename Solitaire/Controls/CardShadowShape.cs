using Avalonia;

namespace Solitaire.Controls;

internal readonly record struct CardShadowShape(Point TopLeft, Point TopRight, Point BottomRight, Point BottomLeft, Size CardSize)
{
    public Rect Bounds => new Rect(TopLeft, TopRight).Normalize()
        .Union(new Rect(BottomLeft, BottomRight).Normalize());

    public CardShadowShape Translate(Vector offset) => new(TopLeft + offset, TopRight + offset,
        BottomRight + offset, BottomLeft + offset, CardSize);
}
