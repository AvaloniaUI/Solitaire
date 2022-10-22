using System;
using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Xaml.Interactivity;

namespace SolitaireAvalonia.Behaviors;

/// <summary>
/// 
/// </summary>
public class CardDragBehavior : Behavior<Control>
{
    private bool _dragStarted;
    private Point _start;

    /// <summary>
    /// 
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<CardDragBehavior, Orientation>(nameof(Orientation));

    /// <summary>
    /// 
    /// </summary>
    public static readonly StyledProperty<double> HorizontalDragThresholdProperty =
        AvaloniaProperty.Register<CardDragBehavior, double>(nameof(HorizontalDragThreshold), 3);

    /// <summary>
    /// 
    /// </summary>
    public static readonly StyledProperty<double> VerticalDragThresholdProperty =
        AvaloniaProperty.Register<CardDragBehavior, double>(nameof(VerticalDragThreshold), 3);

    /// <summary>
    /// 
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public double HorizontalDragThreshold
    {
        get => GetValue(HorizontalDragThresholdProperty);
        set => SetValue(HorizontalDragThresholdProperty, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public double VerticalDragThreshold
    {
        get => GetValue(VerticalDragThresholdProperty);
        set => SetValue(VerticalDragThresholdProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree()
    {
        if (AssociatedObject is null) return;
        AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, PointerReleased, RoutingStrategies.Tunnel);
        AssociatedObject.AddHandler(InputElement.PointerPressedEvent, PointerPressed, RoutingStrategies.Tunnel);
        AssociatedObject.AddHandler(InputElement.PointerMovedEvent, PointerMoved, RoutingStrategies.Tunnel);
        AssociatedObject.AddHandler(InputElement.PointerCaptureLostEvent, PointerCaptureLost,
            RoutingStrategies.Tunnel);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree()
    {
        if (AssociatedObject is null) return;
        AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, PointerReleased);
        AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, PointerPressed);
        AssociatedObject.RemoveHandler(InputElement.PointerMovedEvent, PointerMoved);
        AssociatedObject.RemoveHandler(InputElement.PointerCaptureLostEvent, PointerCaptureLost);
    }

    private void PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var properties = e.GetCurrentPoint(AssociatedObject).Properties;
        if (properties.IsLeftButtonPressed)
        {
            _dragStarted = true;
            _start = e.GetCurrentPoint(AssociatedObject.Parent).Position;
            AddTransforms(AssociatedObject);
            e.Pointer.Capture(AssociatedObject);
        }
    }

    private void PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!Equals(e.Pointer.Captured, AssociatedObject)) return;

        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            Released();
        }

        e.Pointer.Capture(null);
    }

    private void PointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        Released();
    }

    private void Released()
    {
        _dragStarted = false;
        RemoveTransforms(AssociatedObject);
    }

    private void AddTransforms(IControl? control)
    {
        SetTranslateTransform(control, Vector.Zero);
        ((IPseudoClasses)control.Classes).Add(":dragging");
    }

    private void RemoveTransforms(IControl? control)
    {
        ((IPseudoClasses)control.Classes).Remove(":dragging");
        SetTranslateTransform(control, Vector.Zero);
    }
    
    private void PointerMoved(object? sender, PointerEventArgs e)
    {
        var properties = e.GetCurrentPoint(AssociatedObject).Properties;

        if (!Equals(e.Pointer.Captured, AssociatedObject) || !properties.IsLeftButtonPressed || !_dragStarted) return;

        var position = e.GetCurrentPoint(AssociatedObject.Parent).Position;

        var delta = position - _start;

        if (Math.Abs(delta.X) < HorizontalDragThreshold || Math.Abs(delta.Y) < VerticalDragThreshold)
        {
            return;
        }

        SetTranslateTransform(AssociatedObject, delta);
    }

    private void SetTranslateTransform(IControl? control, Vector newVector)
    {
        if (control is null) return;
        var transformBuilder = new TransformOperations.Builder(1);
        transformBuilder.AppendTranslate(newVector.X, newVector.Y);
        control.RenderTransform = transformBuilder.Build();
    }
}