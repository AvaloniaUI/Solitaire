using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Solitaire.Controls;

public class RandomNoiseTextureControl : Control
{
    private const int DesiredWidth = 300;
    private const int DesiredHeight = 300;
    internal static RenderTargetBitmap? NoiseTexture;

    private static readonly Random Rng = new();

    public RandomNoiseTextureControl()
    {
        if (NoiseTexture is not { })
        {
            GenerateNoise();
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(DesiredWidth, DesiredHeight);
    }

    public override void Render(DrawingContext context)
    {
        if (NoiseTexture is null) return;
        context.DrawImage(NoiseTexture, new Rect(0, 0, DesiredWidth, DesiredHeight));
        base.Render(context);
    }

    private void GenerateNoise()
    {
        NoiseTexture = new RenderTargetBitmap(new PixelSize(DesiredWidth, DesiredHeight));

        using var dc = NoiseTexture.CreateDrawingContext();

        for (var i = 0; i < DesiredHeight; i++)
        for (var j = 0; j < DesiredWidth; j++)
        {
            var k = new RoundedRect(new Rect(i, j, 1, 1));
            var c = Rng.NextDouble() > 0.5 ? Brushes.White : Brushes.Black;
            dc.DrawRectangle(c, null, k);
        }
    }
}