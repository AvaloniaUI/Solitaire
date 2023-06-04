using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Solitaire.Behaviors;

public class GoldenPanelBorderBehavior : GoldenPanelBaseBehavior
{
    private bool _animationsEnabled = true;

    protected override void OnAttachedToVisualTree()
    {
        if (AssociatedObject is Border border)
        {
            border.Loaded += BorderOnLoaded;
            border.Unloaded += BorderOnUnload;
        }

        base.OnAttachedToVisualTree();
    }

    private void BorderOnUnload(object? sender, RoutedEventArgs e)
    {
        if (sender is not Border border) return;
        border.LayoutUpdated -= BorderOnLayoutUpdated;
    }

    private void BorderOnLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not Border border) return;
        border.LayoutUpdated += BorderOnLayoutUpdated;
        if (UpdateTrayBoundsAction is null || MasterCanvas is null) return;
        var t = border.TransformToVisual(MasterCanvas);
        if (t is not { } m) return;
        var z = m.Transform(border.Bounds.TopLeft);
        var w = m.Transform(border.Bounds.BottomRight);
        UpdateTrayBoundsAction(new Rect(z, w), _animationsEnabled);
        _animationsEnabled = false;
    }

    private void BorderOnLayoutUpdated(object? sender, EventArgs e)
    {
        if (sender is not Border border || UpdateTrayBoundsAction is null || MasterCanvas is null) return;
        var t = border.TransformToVisual(MasterCanvas);
        if (t is not { } m) return;
        var z = m.Transform(border.Bounds.TopLeft);
        var w = m.Transform(border.Bounds.BottomRight);
        
        UpdateTrayBoundsAction(new Rect(z, w), _animationsEnabled);
    }
}