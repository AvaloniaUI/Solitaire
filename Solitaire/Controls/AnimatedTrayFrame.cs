using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Solitaire.Controls;

internal sealed partial class AnimatedTrayFrame : ConnectedTray
{
    [GeneratedStyledProperty]
    public partial Rect FrameBounds { get; set; }

    [GeneratedStyledProperty]
    public partial Point FramePosition { get; set; }

    [GeneratedStyledProperty]
    public partial Size FrameSize { get; set; }

    [GeneratedStyledProperty(DefaultValue = 1d)]
    public partial double FrameScale { get; set; }
    private readonly MatrixTransform _placement = new();
    private TrayFrameBorder? _outer;
    private TrayFrameBorder? _inner;
    private TrayFrameBorder? _background;

    public AnimatedTrayFrame()
    {
        RenderTransform = _placement;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _outer = e.NameScope.Find<TrayFrameBorder>("PART_Outer");
        _inner = e.NameScope.Find<TrayFrameBorder>("PART_Inner");
        _background = e.NameScope.Find<TrayFrameBorder>("PART_Background");
        UpdateFrame();
    }

    public ConnectedFrameState State => new(FrameBounds, FrameScale, FrameShadow);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == FramePositionProperty || change.Property == FrameSizeProperty)
            SetCurrentValue(FrameBoundsProperty, new Rect(FramePosition, FrameSize));
        if (change.Property == FrameShadowProperty || change.Property == FrameBoundsProperty || change.Property == FrameScaleProperty)
            UpdateFrame();
    }

    private void UpdateFrame()
    {
        if (_outer is null || _inner is null || _background is null || FrameScale <= 0)
            return;
        _outer.BoxShadow = FrameShadow;
        var size = new Size(FrameBounds.Width / FrameScale, FrameBounds.Height / FrameScale);
        _outer.DrawingSize = size;
        _inner.DrawingSize = new Rect(size).Deflate(8).Size;
        _background.DrawingSize = new Rect(size).Deflate(10).Size;
        _placement.Matrix = new Matrix(FrameScale, 0, 0, FrameScale, FrameBounds.X, FrameBounds.Y);
    }
}
