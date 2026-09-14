using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Solitaire.Controls;

// Draw the frame with Border styles without changing layout.
internal sealed partial class TrayFrameBorder : Control
{
    protected override Type StyleKeyOverride => typeof(Border);
    [GeneratedStyledProperty(AddOwnerFrom = typeof(Border))]
    public partial IBrush? Background { get; set; }

    [GeneratedStyledProperty(AddOwnerFrom = typeof(Border))]
    public partial IBrush? BorderBrush { get; set; }

    [GeneratedStyledProperty(AddOwnerFrom = typeof(Border))]
    public partial Thickness BorderThickness { get; set; }

    [GeneratedStyledProperty(AddOwnerFrom = typeof(Border))]
    public partial CornerRadius CornerRadius { get; set; }

    [GeneratedStyledProperty(AddOwnerFrom = typeof(Border))]
    public partial BoxShadows BoxShadow { get; set; }

    [GeneratedStyledProperty]
    public partial Size DrawingSize { get; set; }
    private Pen? _pen;
    private GoldTrayIdleAnimationSession? _idleAnimation;

    static TrayFrameBorder() => AffectsRender<TrayFrameBorder>(BackgroundProperty, BorderBrushProperty,
        BorderThicknessProperty, CornerRadiusProperty, BoxShadowProperty, DrawingSizeProperty);

    public TrayFrameBorder()
    {
        UseLayoutRounding = false;
    }

    public GoldTraySurface? Surface { get; set; }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_idleAnimation is null && Surface is { } surface)
            _idleAnimation = new GoldTrayIdleAnimationSession(this, surface);
        _idleAnimation?.Attach();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _idleAnimation?.Detach();
        base.OnDetachedFromVisualTree(e);
    }

    public override void Render(DrawingContext context)
    {
        if (DrawingSize.Width <= 0 || DrawingSize.Height <= 0)
            return;
        var thickness = GetValue(BorderThicknessProperty).Top;
        var brush = GetValue(BorderBrushProperty);
        if (brush is null || thickness == 0)
            _pen = null;
        else if (_pen is null || !ReferenceEquals(_pen.Brush, brush) || _pen.Thickness != thickness)
            _pen = new Pen(brush, thickness);
        var radius = GetValue(CornerRadiusProperty);
        var bounds = new Rect(DrawingSize).Deflate(thickness * .5);
        // This is Avalonia 12.1.2 BorderRenderHelper's uniform rounded-border drawing path.
        context.DrawRectangle(GetValue(BackgroundProperty), _pen,
            new RoundedRect(bounds, radius.TopLeft, radius.TopRight, radius.BottomRight, radius.BottomLeft), BoxShadow);
    }
}
