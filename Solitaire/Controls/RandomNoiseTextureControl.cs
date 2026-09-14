using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Solitaire.Controls;

public class RandomNoiseTextureControl : Control
{
    private const int DesiredWidth = 300;
    private const int DesiredHeight = 300;
    private static WriteableBitmap? _texture;

    private static readonly Random Rng = new();

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
        if (_texture is not { })
            return;
        context.DrawImage(_texture, new Rect(0, 0, DesiredWidth, DesiredHeight));
        base.Render(context);
    }

    private static void GenerateNoise()
    {
        _texture = new WriteableBitmap(new PixelSize(DesiredWidth, DesiredHeight),
            new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
        using var buffer = _texture.Lock();
        var pixels = new byte[buffer.RowBytes * DesiredHeight];

        for (var i = 0; i < DesiredHeight; i++)
            for (var j = 0; j < DesiredWidth; j++)
            {
                var offset = j * buffer.RowBytes + i * 4;
                var color = Rng.NextDouble() > 0.5 ? (byte)255 : (byte)0;
                pixels[offset] = pixels[offset + 1] = pixels[offset + 2] = color;
                pixels[offset + 3] = 255;
            }
        Marshal.Copy(pixels, 0, buffer.Address, pixels.Length);
    }
}
