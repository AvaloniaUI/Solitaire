using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace Solitaire.Controls;

// Fixed geometry lets the renderer draw while the UI changes.
internal sealed class CardShadowDrawOperation : ICustomDrawOperation
{
    private readonly CardShadowShape[] _cards;
    private readonly float _blur;
    private SKPath? _path;
    private SKPaint? _paint;

    public CardShadowDrawOperation(CardShadowShape[] cards, Rect bounds, double blur)
    {
        _cards = cards;
        _blur = (float)blur;
        Bounds = bounds.Inflate(blur * 3);
    }

    public Rect Bounds { get; }
    public bool HitTest(Point p) => false;
    public bool Equals(ICustomDrawOperation? other) => false;
    public void Dispose()
    {
        _paint?.Dispose();
        _path?.Dispose();
        _paint = null;
        _path = null;
    }

    public void Render(ImmediateDrawingContext context)
    {
        if (context.TryGetFeature<ISkiaSharpApiLeaseFeature>() is not { } feature)
            throw new InvalidOperationException("Card shadows require the Skia renderer.");
        using var lease = feature.Lease();
        // Reuse the path and paint because the geometry does not change.
        _path ??= CreatePath(_cards);
        _paint ??= CreatePaint();
        _paint.Color = SKColors.Black.WithAlpha((byte)Math.Clamp(Math.Round(lease.CurrentOpacity * 255), 0, 255));
        lease.SkCanvas.DrawPath(_path, _paint);
    }

    private SKPaint CreatePaint()
    {
        using var blur = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, _blur);
        return new SKPaint
        {
            IsAntialias = true,
            MaskFilter = blur
        };
    }

    internal static SKPath CreatePath(CardShadowShape[] cards)
    {
        // AddPath loses the Skia rounded-rectangle type, even for an empty path.
        // A single outline keeps that type.
        var path = cards.Length > 0 ? CreateOutline(in cards[0]) : new SKPath();
        for (var i = 1; i < cards.Length; i++)
        {
            using var outline = CreateOutline(in cards[i]);
            path.AddPath(outline);
        }
        return path;
    }

    private static SKPath CreateOutline(in CardShadowShape card)
    {
        var outline = new SKPath();
        outline.AddRoundRect(new SKRect(0, 0, (float)card.CardSize.Width, (float)card.CardSize.Height), 6, 6);
        var horizontal = (card.TopRight - card.TopLeft) / card.CardSize.Width;
        var vertical = (card.BottomLeft - card.TopLeft) / card.CardSize.Height;
        outline.Transform(new SKMatrix
        {
            ScaleX = (float)horizontal.X,
            SkewY = (float)horizontal.Y,
            SkewX = (float)vertical.X,
            ScaleY = (float)vertical.Y,
            TransX = (float)card.TopLeft.X,
            TransY = (float)card.TopLeft.Y,
            Persp2 = 1
        });
        return outline;
    }
}
