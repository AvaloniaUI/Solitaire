using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace Solitaire.Controls;

public sealed class GoldTrayBorder : Border
{
    private GoldTrayIdleAnimationSession? _idleAnimation;

    protected override Type StyleKeyOverride => typeof(Border);

    public GoldTraySurface Surface { get; set; }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _idleAnimation = new GoldTrayIdleAnimationSession(this, Surface);
        _idleAnimation.Attach();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _idleAnimation?.Detach();
        _idleAnimation = null;
        base.OnDetachedFromVisualTree(e);
    }
}
