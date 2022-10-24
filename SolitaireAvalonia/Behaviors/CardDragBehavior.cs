using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media.Transformation;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using DynamicData.Kernel;
using SolitaireAvalonia.Controls;
using SolitaireAvalonia.ViewModels;

namespace SolitaireAvalonia.Behaviors;

/// <summary>
/// 
/// </summary>
public class CardDragBehavior : Behavior<Control>
{
    private bool _dragStarted;
    private Point _start;

    public static readonly AttachedProperty<bool> IsDragSourceProperty =
        AvaloniaProperty.RegisterAttached<CardDragBehavior, CardStackControl, bool>
            ("IsDragSource", defaultValue: true);

    public static void SetIsDragSource(CardStackControl obj, bool value) => obj.SetValue(IsDragSourceProperty, value);
    public static bool GetIsDragSource(CardStackControl obj) => obj.GetValue(IsDragSourceProperty);


    public static readonly AttachedProperty<object?> DragTargetObjectProperty =
        AvaloniaProperty.RegisterAttached<CardDragBehavior, Control, object?>("DragTargetObject");

    public static void SetDragTargetObject(Control obj, object? value) => obj.SetValue(DragTargetObjectProperty, value);
    public static object? GetDragTargetObject(Control obj) => obj.GetValue(DragTargetObjectProperty);


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

        if (AssociatedObject.DataContext is not PlayingCardViewModel pcvm)
        {
            throw new InvalidOperationException(
                $"This behavior cannot be used if the {nameof(AssociatedObject)} doesn't have" +
                $"{nameof(PlayingCardViewModel)} as DataContext");
        }

        AssociatedDataContext = pcvm;

        AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, PointerReleased, RoutingStrategies.Tunnel);
        AssociatedObject.AddHandler(InputElement.PointerPressedEvent, PointerPressed, RoutingStrategies.Tunnel);
        AssociatedObject.AddHandler(InputElement.PointerMovedEvent, PointerMoved, RoutingStrategies.Tunnel);
        AssociatedObject.AddHandler(InputElement.PointerCaptureLostEvent, PointerCaptureLost,
            RoutingStrategies.Tunnel);
    }

    private PlayingCardViewModel AssociatedDataContext { get; set; } = null!;

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
        if (!AssociatedDataContext.IsPlayable) return;

        var properties = e.GetCurrentPoint(AssociatedObject).Properties;
        
        if (!properties.IsLeftButtonPressed && AssociatedObject is null) return;
        
        var itemsParent = AssociatedObject!.GetVisualAncestors()
            .FirstOrOptional(x => x.GetType() == typeof(CardStackControl));

        if (!itemsParent.HasValue || itemsParent.Value is not CardStackControl ip || !GetIsDragSource(ip)) return;
        
        _dragStarted = true;
        _start = e.GetCurrentPoint(AssociatedObject!.Parent).Position;
        AddTransforms();
        e.Pointer.Capture(AssociatedObject);
    }

    private void PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!Equals(e.Pointer.Captured, AssociatedObject) || !_dragStarted) return;

        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            Released(e.GetCurrentPoint(AssociatedObject?.GetVisualRoot()).Position);
        }

        e.Pointer.Capture(null);
    }

    private void PointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        Released();
    }

    private void Released(Point position = new())
    {
        _dragStarted = false;

        if (position != new Point())
        {
            var gi = AssociatedDataContext.CardGameInstance;
            var fromList = gi.GetCardCollection(AssociatedDataContext);

            foreach (var visual in AssociatedObject?.GetVisualRoot()?.GetVisualsAt(position)!)
            {
                if (visual is not Control targetControl) continue;
                if (GetDragTargetObject(targetControl) is not IList<PlayingCardViewModel> toList) continue;
                gi.CheckAndMoveCard(fromList, toList, AssociatedDataContext);
                break;
            }
        }

        RemoveTransforms();
    }

    private void AddTransforms()
    {
        if (AssociatedObject is null) return;
        SetTranslateTransform(AssociatedObject, Vector.Zero);
        ((IPseudoClasses)AssociatedObject.Classes).Add(":dragging");
    }

    private void RemoveTransforms()
    {
        if (AssociatedObject is null) return;

        ((IPseudoClasses)AssociatedObject.Classes).Remove(":dragging");
        SetTranslateTransform(AssociatedObject, Vector.Zero);
    }

    private void PointerMoved(object? sender, PointerEventArgs e)
    {
        var properties = e.GetCurrentPoint(AssociatedObject).Properties;

        if (!Equals(e.Pointer.Captured, AssociatedObject) || !properties.IsLeftButtonPressed || !_dragStarted ||
            AssociatedObject is null) return;

        var position = e.GetCurrentPoint(AssociatedObject.Parent).Position;

        var delta = position - _start;

        if (Math.Abs(delta.X) < HorizontalDragThreshold || Math.Abs(delta.Y) < VerticalDragThreshold)
        {
            return;
        }

        SetTranslateTransform(AssociatedObject, delta);
        
        
        var gi = AssociatedDataContext.CardGameInstance;
        var fromList = gi.GetCardCollection(AssociatedDataContext);

        if (AssociatedObject?.Parent is CardStackControl cardStackControl)
        {
        }
         
        
        
        
    }

    private static void SetTranslateTransform(IControl? control, Vector newVector)
    {
        if (control is null) return;
        var transformBuilder = new TransformOperations.Builder(1);
        transformBuilder.AppendTranslate(newVector.X, newVector.Y);
        control.RenderTransform = transformBuilder.Build();
    }
}