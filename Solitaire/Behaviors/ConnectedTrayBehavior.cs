using System;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Rendering.Composition;
using Avalonia.Xaml.Interactivity;
using static Avalonia.Controls.Control;

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
        UpdateTrayBoundsAction(new Rect(z, w));
    }

    private void BorderOnLayoutUpdated(object? sender, EventArgs e)
    {
        if (sender is not Border border || UpdateTrayBoundsAction is null || MasterCanvas is null) return;
        var t = border.TransformToVisual(MasterCanvas);
        if (t is not { } m) return;
        var z = m.Transform(border.Bounds.TopLeft);
        var w = m.Transform(border.Bounds.BottomRight);
        UpdateTrayBoundsAction(new Rect(z, w));
    }

    private void CanvasOnLoaded(object? sender, RoutedEventArgs e)
    {
        UpdateTrayBoundsAction = UpdateTrayBounds;
        MasterCanvas = sender as Canvas ?? throw new InvalidOperationException();


        if (ElementComposition.GetElementVisual(MasterCanvas.Children[0]) is { } compositionVisual)
        {
            var compositor = compositionVisual.Compositor;

            var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.Target = "Offset";
            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(400);

            var sizeAnimation = compositor.CreateVector2KeyFrameAnimation();
            sizeAnimation.Target = "Size";
            sizeAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            sizeAnimation.Duration = TimeSpan.FromMilliseconds(400);

            var animationGroup = compositor.CreateAnimationGroup();
            animationGroup.Add(offsetAnimation);
            animationGroup.Add(sizeAnimation);

            var implicitAnimations = compositor.CreateImplicitAnimationCollection();
            implicitAnimations["Offset"] = animationGroup;

            compositionVisual.ImplicitAnimations = implicitAnimations;
        }
    }

    private static Rect _lastCallRect;

    private void UpdateTrayBounds(Rect obj)
    {
        if (AssociatedObject is not Canvas { Children.Count: 1 } canvas || _lastCallRect.Equals(obj)) return;
        var target = canvas.Children[0];

        Canvas.SetTop(target, obj.Top);
        Canvas.SetLeft(target, obj.Left);
        target.SetValue(Layoutable.WidthProperty, obj.Width, BindingPriority.Animation);
        target.SetValue(Layoutable.HeightProperty, obj.Height, BindingPriority.Animation);

        _lastCallRect = obj;
    }
}