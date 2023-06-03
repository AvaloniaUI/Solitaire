using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Utilities;

namespace Solitaire.Behaviors;

internal class BorderRenderHelper
{
    public static void Render(ImmediateDrawingContext context,
        Rect rect, Thickness borderThickness, int cornerRadius,
        IBrush? background, IBrush? borderBrush, BoxShadows boxShadows, double borderDashOffset = 0,
        PenLineCap borderLineCap = PenLineCap.Flat, PenLineJoin borderLineJoin = PenLineJoin.Miter,
        IReadOnlyCollection<double>? borderDashArray = null)
    {
        var bordThick = borderThickness.Top;
        IPen? pen = null;

        ImmutableDashStyle? dashStyle = null;

        if (borderDashArray is { Count: > 0 })
        {
            dashStyle = new ImmutableDashStyle(borderDashArray, borderDashOffset);
        }

        if (borderBrush != null && bordThick > 0)
        {
            pen = new ImmutablePen(
                borderBrush.ToImmutable(),
                bordThick,
                dashStyle,
                borderLineCap,
                borderLineJoin);
        }

        if (!MathUtilities.IsZero(bordThick))
            rect = rect.Deflate(bordThick * 0.5);
        if (background == null || pen == null) return;
        context.DrawRectangle(background.ToImmutable(), pen.ToImmutable(), rect, cornerRadius, cornerRadius,
            boxShadows);
    }
}