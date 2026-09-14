using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;

namespace Solitaire.Controls;

// Cache the card artwork separately from movement and shadows.
public sealed class CardArtworkPanel : Panel
{
    protected override Type StyleKeyOverride => typeof(Panel);

    public CardArtworkPanel()
    {
        RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.HighQuality);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        CacheMode = new BitmapCache();
        InvalidateVisual();
        LayoutUpdated += UpdateCacheScale;
        UpdateCacheScale(this, EventArgs.Empty);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        LayoutUpdated -= UpdateCacheScale;
        CacheMode = null;
        InvalidateVisual();
        base.OnDetachedFromVisualTree(e);
    }

    private void UpdateCacheScale(object? sender, EventArgs e)
    {
        if (CacheMode is not BitmapCache cache || TopLevel.GetTopLevel(this) is not { } topLevel ||
            this.TransformToVisual(topLevel) is not { } transform)
            return;
        var horizontal = Math.Sqrt(transform.M11 * transform.M11 + transform.M12 * transform.M12);
        var vertical = Math.Sqrt(transform.M21 * transform.M21 + transform.M22 * transform.M22);
        // Avalonia adds the display scale. Use at least two pixels per logical pixel.
        // The vertical scale stays constant during horizontal turnover.
        var scale = Math.Max(1, 2 / topLevel.RenderScaling) * Math.Max(horizontal, vertical);
        if (double.IsFinite(scale) && scale > 0 && Math.Abs(cache.RenderAtScale - scale) > .001)
            cache.RenderAtScale = scale;
    }
}
