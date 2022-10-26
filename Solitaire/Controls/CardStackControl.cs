using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.Styling;
using Avalonia.VisualTree;
using ReactiveUI;
using Solitaire.ViewModels;
using Vector = Avalonia.Vector;

namespace Solitaire.Controls;

public class CardStackControl : Border, IStyleable
{
    public CardStackControl()
    {
        if (Application.Current != null &&
            Application.Current.Styles.TryGetResource("PlayingCardDataTemplate", out var obj) &&
            obj is DataTemplate dataTemplate)
        {
            _dataTemplate = dataTemplate;
        }

        this.WhenAnyValue(x => x.SourceItems)
            .Do(x =>
            {
                if (x is { })
                    x.CollectionChanged += XOnCollectionChanged;
            })
            .Subscribe();
    }


    private void EnsureImplicitAnimations()
    {
        if (ImplicitAnimations != null)
        {
            return;
        }

        var compositor = ElementComposition.GetElementVisual(this)!.Compositor;

        var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.Target = "Offset";
        offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        offsetAnimation.Duration = TimeSpan.FromMilliseconds(400);

        // var rotationAnimation = compositor.CreateScalarKeyFrameAnimation();
        // rotationAnimation.Target = "RotationAngle";
        // rotationAnimation.InsertKeyFrame(0.0f, 0.0f);
        // rotationAnimation.InsertKeyFrame(1.0f, (float) (Math.PI * 2.0));
        // rotationAnimation.Duration = TimeSpan.FromMilliseconds(400);

        var animationGroup = compositor.CreateAnimationGroup();
        animationGroup.Add(offsetAnimation);
        // animationGroup.Add(rotationAnimation);

        ImplicitAnimations = compositor.CreateImplicitAnimationCollection();
        ImplicitAnimations["Offset"] = animationGroup;
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        EnsureImplicitAnimations();
        base.OnAttachedToVisualTree(e);
    }

    protected override void OnLoaded()
    {
        foreach (var keyValues in _containers)
        {
            SetContainerLayout(keyValues.Key, keyValues.Value);
        }

        base.OnLoaded();
    }

    private readonly Dictionary<PlayingCardViewModel, ContentControl> _containers = new();

    private void XOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (TargetCanvas is null || _dataTemplate is null) return;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:

                if (e.NewItems is { })
                {
                    foreach (var newItem in e.NewItems.Cast<PlayingCardViewModel>())
                    {
                        var cachedContainer = new ContentControl()
                        {
                            DataTemplates = {_dataTemplate},
                            Content = newItem
                        };

                        newItem.PropertyChanged += NewItemOnPropertyChanged;


                        _containers.TryAdd(newItem, cachedContainer);

                        RegisterEvents(cachedContainer);

                        TargetCanvas.Children.Add(cachedContainer);
                    }

                    foreach (var keyValues in _containers)
                    {
                        SetContainerLayout(keyValues.Key, keyValues.Value);
                    }
                }


                break;
            case NotifyCollectionChangedAction.Remove:

                if (e.OldItems is { })
                    foreach (var oldItem in e.OldItems.Cast<PlayingCardViewModel>())
                    {
                        var container = _containers[oldItem];
                        oldItem.PropertyChanged -= NewItemOnPropertyChanged;
                        TargetCanvas.Children.Remove(container);
                        _containers.Remove(oldItem);
                        UnregisterEvents(container);
                    }

                break;
            case NotifyCollectionChangedAction.Replace:
                break;
            case NotifyCollectionChangedAction.Move:
                break;
            case NotifyCollectionChangedAction.Reset:

                foreach (var container in TargetCanvas.Children.Cast<ContentControl>().ToList())
                {
                    if (container is null) continue;
                    var card = _containers.FirstOrDefault(x => x.Value.Equals(container)).Key;
                    if (card is not { }) continue;

                    card.PropertyChanged -= NewItemOnPropertyChanged;
                    TargetCanvas.Children.Remove(container);
                    _containers.Remove(card);
                    UnregisterEvents(container);
                }


                break;
        }
    }

    private void RegisterEvents(ContentControl container)
    {
        container.AttachedToVisualTree += ContainerOnAttachedToVisualTree;
        container.PointerPressed += ContainerOnPointerPressed;
        container.PointerMoved += ContainerOnPointerMoved;
        container.PointerReleased += ContainerOnPointerReleased;
        container.PointerCaptureLost += ContainerOnPointerCaptureLost;
    }

    private void UnregisterEvents(ContentControl container)
    {
        container.AttachedToVisualTree -= ContainerOnAttachedToVisualTree;
        container.PointerPressed -= ContainerOnPointerPressed;
        container.PointerMoved -= ContainerOnPointerMoved;
        container.PointerReleased -= ContainerOnPointerReleased;
        container.PointerCaptureLost -= ContainerOnPointerCaptureLost;
    }

    private void ContainerOnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is ContentControl container && ElementComposition.GetElementVisual(container) is
                { } compositionVisual)
        {
            AddImplicitAnimations(container);
        }
    }

    private void AddImplicitAnimations(ContentControl container)
    {
        if (ElementComposition.GetElementVisual(container) is { } compositionVisual)
        {
            compositionVisual.ImplicitAnimations = ImplicitAnimations;
        }
    }

    private void RemoveImplicitAnimations(ContentControl container)
    {
        if (ElementComposition.GetElementVisual(container) is { } compositionVisual)
        {
            compositionVisual.ImplicitAnimations = null;
        }
    }

    private void ContainerOnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (!IsDragging) return;

        var container = sender as ContentControl;
        var card = container.Content as PlayingCardViewModel;

        container.ZIndex = _startZIndex;
        AddImplicitAnimations(container);
        Canvas.SetLeft(container, _startVector.X);
        Canvas.SetTop(container, _startVector.Y);

        IsDragging = false;
    }

    private void ContainerOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!IsDragging) return;

        var container = sender as ContentControl;
        var card = container.Content as PlayingCardViewModel;

        container.ZIndex = _startZIndex;
        AddImplicitAnimations(container);
        Canvas.SetLeft(container, _startVector.X);
        Canvas.SetTop(container, _startVector.Y);

        IsDragging = false;

        e.Pointer.Capture(null);
    }

    private void ContainerOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!IsDragging) return;

        var container = sender as ContentControl;
        var card = container.Content as PlayingCardViewModel;


        var properties = e.GetCurrentPoint(container).Properties;

        if (!Equals(e.Pointer.Captured, container) || !properties.IsLeftButtonPressed || !IsDragging) return;

        var position = e.GetCurrentPoint(container.Parent).Position;

        var delta = position - _start;

        if (Math.Abs(delta.X) < 3 || Math.Abs(delta.Y) < 3)
        {
            return;
        }

        Canvas.SetLeft(container, (_startVector.X + delta.X));
        Canvas.SetTop(container, (_startVector.Y + delta.Y));
    }

    private void ContainerOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        ExecuteCardClickCommand();

        if (IsDragging) return;

        var container = sender as ContentControl;
        var card = container.Content as PlayingCardViewModel;


        if (!card.IsPlayable) return;


        var properties = e.GetCurrentPoint(container).Properties;

        if (!properties.IsLeftButtonPressed) return;

        _start = e.GetCurrentPoint(container!.Parent).Position;
        e.Pointer.Capture(container);
        RemoveImplicitAnimations(container);
        _startZIndex = container.ZIndex;

        container.ZIndex = int.MaxValue;

        IsDragging = true;
    }


    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        ExecuteCardClickCommand();
        base.OnPointerPressed(e);
    }

    private void ExecuteCardClickCommand()
    {
        if (CommandOnCardClick is null || !CommandOnCardClick.CanExecute(null)) return;
        CommandOnCardClick.Execute(null);
    }

    private void NewItemOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PlayingCardViewModel.IsFaceDown) && sender is PlayingCardViewModel card)
        {
            SetContainerLayout(card, _containers[card]);
        }
    }

    private void SetContainerLayout(PlayingCardViewModel card, ContentControl cachedContainer)
    {
        if (SourceItems is null) return;

        var cardIndex = SourceItems.IndexOf(card);
        var totalItems = SourceItems.Count;

        double totalOffset = 0;

        for (var i = 0; i <= cardIndex; i++)
        {
            if (i - 1 < 0) continue;

            var z = SourceItems[i - 1];
            GetOffsets(z, i, totalItems, out var a, out var b);
            var c = z.IsFaceDown ? a : b;
            totalOffset += c;
        }


        var compositionVisual = ElementComposition.GetElementVisual(cachedContainer);
        if (compositionVisual is null)
        {
            return;
        }

        var compositor = compositionVisual.Compositor;
        var animation = compositor.CreateVector3KeyFrameAnimation();

        _startVector = Orientation == Orientation.Horizontal
            ? new Vector((float) Bounds.Left + (float) totalOffset, (float) Bounds.Top)
            : new Vector((float) Bounds.Left, (float) Bounds.Top + (float) totalOffset);

        animation.InsertKeyFrame(1f,
            new Vector3((float) _startVector.X, (float) _startVector.Y, 0f));
        animation.Duration = TimeSpan.FromSeconds(0.4);

        compositionVisual.StartAnimation("Offset", animation);
    }


    private void GetOffsets(PlayingCardViewModel card, int n, int total, out double faceDownOffset,
        out double faceUpOffset)

    {
        faceDownOffset = 0;
        faceUpOffset = 0;

        //  We are now going to offset only if the offset mode is appropriate.
        switch (OffsetMode)
        {
            case OffsetMode.EveryCard:
                //  Offset every card.
                faceDownOffset = FaceDownOffset;
                faceUpOffset = FaceUpOffset;
                break;
            case OffsetMode.EveryNthCard:
                //  Offset only if n Mod N is zero.
                if ((n + 1) % NValue == 0)
                {
                    faceDownOffset = FaceDownOffset;
                    faceUpOffset = FaceUpOffset;
                }

                break;
            case OffsetMode.TopNCards:
                //  Offset only if (Total - N) <= n < Total
                if (n > (total - NValue))
                {
                    faceDownOffset = FaceDownOffset;
                    faceUpOffset = FaceUpOffset;
                }

                break;

            case OffsetMode.BottomNCards:
                //  Offset only if 0 < n < N
                if (n < NValue)
                {
                    faceDownOffset = FaceDownOffset;
                    faceUpOffset = FaceUpOffset;
                }

                break;
            case OffsetMode.UseCardValues:
                //  Offset each time by the amount specified in the card object.
                faceDownOffset = card.FaceDownOffset;
                faceUpOffset = card.FaceUpOffset;
                break;
        }
    }


    public static readonly StyledProperty<Canvas?> TargetCanvasProperty =
        AvaloniaProperty.Register<CardStackControl, Canvas?>(
            "TargetCanvas");

    public Canvas? TargetCanvas
    {
        get => GetValue(TargetCanvasProperty);
        set => SetValue(TargetCanvasProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<PlayingCardViewModel>?> SourceItemsProperty =
        AvaloniaProperty.Register<CardStackControl, ObservableCollection<PlayingCardViewModel>?>(
            "SourceItems");

    public ObservableCollection<PlayingCardViewModel>? SourceItems
    {
        get => GetValue(SourceItemsProperty);
        set => SetValue(SourceItemsProperty, value);
    }

    Type IStyleable.StyleKey => typeof(CardStackControl);

    public static readonly StyledProperty<double> FaceDownOffsetProperty =
        AvaloniaProperty.Register<CardStackControl, double>(
            "FaceDownOffset");

    public double FaceDownOffset
    {
        get => GetValue(FaceDownOffsetProperty);
        set => SetValue(FaceDownOffsetProperty, value);
    }

    public static readonly StyledProperty<double> FaceUpOffsetProperty =
        AvaloniaProperty.Register<CardStackControl, double>(
            "FaceUpOffset");

    public double FaceUpOffset
    {
        get => GetValue(FaceUpOffsetProperty);
        set => SetValue(FaceUpOffsetProperty, value);
    }

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<CardStackControl, Orientation>(
            "Orientation");

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }


    public static readonly StyledProperty<OffsetMode> OffsetModeProperty =
        AvaloniaProperty.Register<CardStackControl, OffsetMode>(
            "OffsetMode");

    public OffsetMode OffsetMode
    {
        get => GetValue(OffsetModeProperty);
        set => SetValue(OffsetModeProperty, value);
    }

    public static readonly StyledProperty<int> NValueProperty = AvaloniaProperty.Register<CardStackControl, int>(
        "NValue", 1);

    private readonly DataTemplate? _dataTemplate;

    public static readonly StyledProperty<ICommand?> CommandOnCardClickProperty =
        AvaloniaProperty.Register<CardStackControl, ICommand?>("CommandOnCardClick");

    private static bool IsDragging;
    private Point _start;
    private Vector _startVector;
    private static ImplicitAnimationCollection? ImplicitAnimations;
    private int _startZIndex;

    public int NValue
    {
        get => GetValue(NValueProperty);
        set => SetValue(NValueProperty, value);
    }

    public ICommand? CommandOnCardClick
    {
        get => (ICommand?) GetValue(CommandOnCardClickProperty);
        set => SetValue(CommandOnCardClickProperty, value);
    }
}