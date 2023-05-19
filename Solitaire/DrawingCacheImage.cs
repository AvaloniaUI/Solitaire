using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Solitaire;

public class DrawingImageCache
{
    class CacheEntry : IImage
    {
        private DrawingImage _image;
        public double Scaling { get; set; } = 1;
        private double _oldScaling;
        private RenderTargetBitmap? _rtb;

        public CacheEntry(DrawingImage image)
        {
            _image = image;
        }

        public void Draw(DrawingContext context, Rect sourceRect, Rect destRect)
        {
            if (_oldScaling != Scaling || destRect.Size != _rtb?.Size)
            {
                _rtb?.Dispose();
                _rtb = null;
            }

            if (_rtb == null)
            {
                _rtb = new RenderTargetBitmap(PixelSize.FromSize(destRect.Size, Scaling),
                    new Vector(96 * Scaling, 96 * Scaling));
                using (var ctx = _rtb.CreateDrawingContext()) 
                    ctx.DrawImage(_image, new Rect(destRect.Size));
            }

            context.DrawImage(_rtb, destRect);

        }

        public Size Size => _image.Size;
    }

    private static readonly Dictionary<DrawingImage, CacheEntry> Cache = new();
    public static IImage GetImageForDrawing(DrawingImage image, double scaling)
    {
        if (!Cache.TryGetValue(image, out var entry))
            Cache[image] = entry = new CacheEntry(image);
        entry.Scaling = scaling;
        return entry;

    }
    
}