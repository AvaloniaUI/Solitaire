using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Solitaire.Controls;

public sealed partial class GoldButtonChrome : ContentControl, IDisposable
{
    [GeneratedStyledProperty(AddOwnerFrom = typeof(Border))]
    public partial BoxShadows BoxShadow { get; set; }

    [GeneratedStyledProperty]
    public partial bool Active { get; set; }

    [GeneratedStyledProperty]
    public partial bool Pressed { get; set; }

    [GeneratedStyledProperty]
    public partial double FocusAmount { get; set; }

    [GeneratedStyledProperty]
    public partial double PressDepth { get; set; }

    [GeneratedStyledProperty]
    public partial double GlintProgress { get; set; }

    [GeneratedStyledProperty(DefaultValue = 1d)]
    public partial double PressSweep { get; set; }

    private readonly GoldControlAnimation _animation;
    private bool _attached;

    public GoldButtonChrome() => _animation = new GoldControlAnimation(this);

    internal bool HasRunningEffects => _animation.HasRunningEffects;
    internal bool IsResting => _animation.IsResting;

    public void Flash() { if (_attached && IsEffectivelyVisible) _animation.Press(); }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ActiveProperty || change.Property == IsVisibleProperty)
        {
            if (_attached)
                _animation.SetActive(Active && IsEffectivelyVisible);
            if (!IsEffectivelyVisible)
                _animation.Stop();
        }
        else if (change.Property == PressedProperty)
        {
            if (Pressed)
                Flash();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _attached = true;
        _animation.SetActive(Active && IsEffectivelyVisible);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _attached = false;
        _animation.Dispose();
        base.OnDetachedFromVisualTree(e);
    }

    public void Dispose() => _animation.Dispose();
}
