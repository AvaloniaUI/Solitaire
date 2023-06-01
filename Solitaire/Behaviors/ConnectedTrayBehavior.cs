using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace Solitaire.Behaviors;

public class ConnectedTrayBehavior : Behavior<Control>
{
    private static Action<Rect>? UpdateTrayBoundsAction { get; set; }
    private static Canvas? MasterCanvas { get; set; }

    protected override void OnAttachedToVisualTree()
    {
        switch (AssociatedObject)
        {
            case Canvas:
                AssociatedObject.Loaded += CanvasOnLoaded;
                break;
            case Border:
                AssociatedObject.Loaded += BorderOnLoaded;
                AssociatedObject.Unloaded += BorderOnUnload;
                break;
        }

        base.OnAttachedToVisualTree();
    }

    private void BorderOnUnload(object? sender, RoutedEventArgs e)
    {
        if (sender is Border border)
        {
            border.LayoutUpdated -= BorderOnLayoutUpdated;
        }
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
        UpdateTrayBoundsAction(new Rect(z,w));
    }

    private void BorderOnLayoutUpdated(object? sender, EventArgs e)
    {
        if (sender is not Border border || UpdateTrayBoundsAction is null || MasterCanvas is null) return;
        var t = border.TransformToVisual(MasterCanvas);
        if (t is not { } m) return;
        var z = m.Transform(border.Bounds.TopLeft);
        var w = m.Transform(border.Bounds.BottomRight);
        UpdateTrayBoundsAction(new Rect(z,w));
    }

    private void CanvasOnLoaded(object? sender, RoutedEventArgs e)
    {
        UpdateTrayBoundsAction = UpdateTrayBounds;
        MasterCanvas = sender as Canvas ?? throw new InvalidOperationException();
    }
    
    private void UpdateTrayBounds(Rect obj)
    {
        if (AssociatedObject is not Canvas { Children.Count: 1 } canvas) return;
        var target = canvas.Children[0];
        Canvas.SetTop(target, obj.Top); 
        Canvas.SetLeft(target, obj.Left);
        target.Width = obj.Width;
        target.Height = obj.Height;
    }
}