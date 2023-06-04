using System;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.Xaml.Interactivity;
using Solitaire.Controls;

namespace Solitaire.Behaviors;

public class GoldenPanelBorderBehavior : GoldenPanelBaseBehavior
{
    private bool _animationsEnabled;

    protected override void OnAttachedToVisualTree()
    {
        if (AssociatedObject is Border border)
        {
            border.Loaded += BorderOnLoaded;
            border.Unloaded += BorderOnUnload;
            _animationsEnabled = true;
        }

        base.OnAttachedToVisualTree();
    }

    private void BorderOnUnload(object? sender, RoutedEventArgs e)
    {
        if (sender is not Border border) return;
        border.LayoutUpdated -= BorderOnLayoutUpdated;
        _animationsEnabled = true;
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
        _animationsEnabled = true;
        UpdateTrayBoundsAction(new Rect(z, w), _animationsEnabled);
    }

    private void BorderOnLayoutUpdated(object? sender, EventArgs e)
    {
        if (sender is not Border border || UpdateTrayBoundsAction is null || MasterCanvas is null) return;
        var t = border.TransformToVisual(MasterCanvas);
        if (t is not { } m) return;
        var z = m.Transform(border.Bounds.TopLeft);
        var w = m.Transform(border.Bounds.BottomRight);

        if (_animationsEnabled)
        {
            _animationsEnabled = false;
        }

        UpdateTrayBoundsAction(new Rect(z, w), _animationsEnabled);
    }
}