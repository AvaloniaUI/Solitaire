using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace SolitaireAvalonia.Controls;

public class RandomNoiseTextureControl : Control
{
    private const int DesiredWidth = 300;
    private const int DesiredHeight = 300;
    private RenderTargetBitmap? _texture = default;

    private static Random rng = new();

    public RandomNoiseTextureControl()
    {
        if (_texture is not { })
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
        context.DrawImage(_texture, new Rect(0, 0, DesiredWidth, DesiredHeight));


        base.Render(context);
    }

    private void GenerateNoise()
    {
        _texture = new RenderTargetBitmap(new PixelSize(DesiredWidth, DesiredHeight));

        using var dc = _texture.CreateDrawingContext(null);

        for (var i = 0; i < DesiredHeight; i++)
        for (var j = 0; j < DesiredWidth; j++)
        {
            var k = new RoundedRect(new Rect(i, j, 1, 1));
            var c = rng.NextDouble() > 0.5 ? Brushes.White : Brushes.Black;
            dc.DrawRectangle(c, null, k);
        }
    }
}