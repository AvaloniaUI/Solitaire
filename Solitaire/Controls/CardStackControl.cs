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
using Avalonia.LogicalTree;
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
    public record ContainerState(Point dragStartPoint, Vector homePosition, int defaultZIndex);

    public CardStackControl()
    {
        if (Application.Current != null &&
            Application.Current.Styles.TryGetResource("PlayingCardDataTemplate", out var obj) &&
            obj is DataTemplate dataTemplate)
        {
            _dataTemplate = dataTemplate;
        }

        sub = this.WhenAnyValue(x => x.SourceItems)
            .Do(x =>
            {
                if (x is { })
                    x.CollectionChanged += XOnCollectionChanged;
            })
            .Subscribe();
    }

    private IDisposable sub;

    private void EnsureImplicitAnimations()
    {
        
        if (isDisposed) 
            return;
        
        if (ImplicitAnimations != null)
        {
            return;
        }

        var compositor = ElementComposition.GetElementVisual(this)!.Compositor;

        var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.Target = "Offset";
        offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        offsetAnimation.Duration = TimeSpan.FromMilliseconds(400);

        var animationGroup = compositor.CreateAnimationGroup();
        animationGroup.Add(offsetAnimation);

        ImplicitAnimations = compositor.CreateImplicitAnimationCollection();
        ImplicitAnimations["Offset"] = animationGroup;
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (isDisposed) 
            return;
        
        EnsureImplicitAnimations();
        base.OnAttachedToVisualTree(e);
    }

    private List<ContentControl> SourceItemContainers => TargetCanvas?.Children.Cast<ContentControl>()
        .Where(x => SourceItems != null && x.Content is PlayingCardViewModel xxx && SourceItems.Contains(xxx))
        .ToList() ?? new List<ContentControl>();

    protected override void OnLoaded()
    {
        if (isDisposed) 
            return;

        foreach (var keyValues in SourceItemContainers)
        {
            SetContainerLayout(keyValues.Content as PlayingCardViewModel, keyValues);
        }

        base.OnLoaded();
    }


    public static readonly AttachedProperty<Dictionary<PlayingCardViewModel, ContentControl>> ContainerCacheProperty =
        AvaloniaProperty.RegisterAttached<CardStackControl, Control, Dictionary<PlayingCardViewModel, ContentControl>>(
            "ContainerCache",
            inherits: true);

    public static void SetContainerCache(Control obj, Dictionary<PlayingCardViewModel, ContentControl> value) =>
        obj.SetValue(ContainerCacheProperty, value);

    public static Dictionary<PlayingCardViewModel, ContentControl> GetContainerCache(Control obj) =>
        obj.GetValue(ContainerCacheProperty);


    private static Dictionary<PlayingCardViewModel, ContainerState> ContainerStates =
        new();

    private static Dictionary<PlayingCardViewModel, CardStackControl> Stacks =
        new();

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ContainerStates.Clear();
        Stacks.Clear();
        var op = GetContainerCache(this);
        foreach (var xd in op)
        {
            UnregisterEvents(xd.Value);
        }

        op.Clear();
        TargetCanvas?.Children.Clear();
        sub?.Dispose();

        base.OnDetachedFromVisualTree(e);

        isDisposed = true;
    }

    private bool isDisposed;

    private void XOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (TargetCanvas is null || _dataTemplate is null || isDisposed) return;
        if (SourceItems is null)
            return;

        foreach (var x in e.NewItems?.Cast<PlayingCardViewModel>() ?? Enumerable.Empty<PlayingCardViewModel>())
        {
            Stacks[x] = this;
        }

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:

                if (e.NewItems is { })
                {
                    foreach (var card in e.NewItems.Cast<PlayingCardViewModel>().OrderBy(x => SourceItems.IndexOf(x)))
                    {
                        var containerCache = GetContainerCache(this);

                        if (card is null)
                            return;

                        var u = containerCache.FirstOrDefault(x => x.Key.CardType == card.CardType);
                        if (u is {Value: { } cached})
                        {
                            cached.ZIndex = SourceItems.IndexOf(card);
                            cached.Content = card;
                            SetContainerLayout(card, cached);
                            continue;
                        }

                        var container = new ContentControl()
                        {
                            DataTemplates = {_dataTemplate},
                            Content = card
                        };

                        card.PropertyChanged += NewItemOnPropertyChanged;
                        RegisterEvents(container);
                        AddImplicitAnimations(container);
                        TargetCanvas.Children.Add(container);
                        containerCache.Add(card, container);
                        SetContainerLayout(card, container);
                    }
                }

                break;
            default:
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

    private static void ContainerOnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
    }

    private static void AddImplicitAnimations(ContentControl container)
    {
        if (ElementComposition.GetElementVisual(container) is { } compositionVisual)
        {
            compositionVisual.ImplicitAnimations = ImplicitAnimations;
        }
    }

    private static void RemoveImplicitAnimations(ContentControl container)
    {
        if (ElementComposition.GetElementVisual(container) is { } compositionVisual)
        {
            compositionVisual.ImplicitAnimations = null;
        }
    }

    private static void ContainerOnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (!IsDragging) return;

        var container = sender as ContentControl;
        var card = container!.Content as PlayingCardViewModel;
        var cs = ContainerStates[card!];

        container.ZIndex = cs.defaultZIndex;
        AddImplicitAnimations(container);
        Canvas.SetLeft(container, cs.homePosition.X);
        Canvas.SetTop(container, cs.homePosition.Y);

        IsDragging = false;
    }

    private static void ContainerOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!IsDragging) return;

        var sourceContainer = sender as ContentControl;
        var sourceCard = sourceContainer.Content as PlayingCardViewModel;

        var vr = sourceContainer.GetVisualRoot()!;
        var y = e.GetCurrentPoint(vr.GetVisualRoot()).Position;
        var cs = ContainerStates[sourceCard];

        foreach (var visual in vr.GetVisualsAt(y))
        {
            if (visual is CardStackControl cardStackControl)
            {
                if (sourceContainer.Parent!.DataContext is CardGameViewModel cardGame)
                {
                    var tL = cardStackControl.SourceItems;
                    var sL = cardGame.GetCardCollection(sourceCard);

                    var hasMoved = cardGame.CheckAndMoveCard(sL, tL, sourceCard);

                    IsDragging = false;
                    sourceContainer.ZIndex = cs.defaultZIndex;
                    AddImplicitAnimations(sourceContainer);

                    if (!hasMoved)
                    {
                        Canvas.SetLeft(sourceContainer, cs.homePosition.X);
                        Canvas.SetTop(sourceContainer, cs.homePosition.Y);
                    }

                    e.Pointer.Capture(null);
                    return;
                }
            }
            else
            {
                continue;
            }

            break;
        }

        sourceContainer.ZIndex = cs.defaultZIndex;
        AddImplicitAnimations(sourceContainer);
        Canvas.SetLeft(sourceContainer, cs.homePosition.X);
        Canvas.SetTop(sourceContainer, cs.homePosition.Y);

        IsDragging = false;

        e.Pointer.Capture(null);
    }

    private static void ContainerOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!IsDragging) return;

        var container = sender as ContentControl;
        var card = container!.Content as PlayingCardViewModel;


        var properties = e.GetCurrentPoint(container).Properties;

        if (!Equals(e.Pointer.Captured, container) || !properties.IsLeftButtonPressed || !IsDragging) return;

        var position = e.GetCurrentPoint(container.Parent).Position;

        var cs = ContainerStates[card!];

        var delta = position - cs.homePosition;

        if (Math.Abs(delta.X) < 3 || Math.Abs(delta.Y) < 3)
        {
            return;
        }

        Canvas.SetLeft(container, (cs.homePosition.X + delta.X));
        Canvas.SetTop(container, (cs.homePosition.Y + delta.Y));
    }

    private static void ContainerOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var container = sender as ContentControl;
        var card = container!.Content as PlayingCardViewModel;

        Stacks[card!].ExecuteCardClickCommand();

        if (IsDragging) return;
        if (!card!.IsPlayable) return;


        var properties = e.GetCurrentPoint(container).Properties;

        if (!properties.IsLeftButtonPressed) return;

        var start = e.GetCurrentPoint(container!.Parent).Position;
        e.Pointer.Capture(container);
        RemoveImplicitAnimations(container);
        var startZIndex = container.ZIndex;

        ContainerStates[card!] = new ContainerState(start, ContainerStates[card!].homePosition, startZIndex);


        container.ZIndex = int.MaxValue;

        IsDragging = true;
    }


    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        
        if (isDisposed) 
            return;
        
        ExecuteCardClickCommand();
        base.OnPointerPressed(e);
    }

    private void ExecuteCardClickCommand()
    {
        if (CommandOnCardClick is null || !CommandOnCardClick.CanExecute(null)) return;
        CommandOnCardClick.Execute(null);
    }

    private static void NewItemOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is PlayingCardViewModel card)
        {
            var homeStack = Stacks[card];

            var f = homeStack.TargetCanvas!.Children
                .Cast<ContentControl>()
                .First(x => x.Content is PlayingCardViewModel s && s.Equals(card));

            SetContainerLayout(card, f);
        }
    }

    private static void SetContainerLayout(PlayingCardViewModel? card, ContentControl? cachedContainer)
    {
        var sourceItems = card.CardGameInstance.GetCardCollection(card);
        var parentCardStock = Stacks[card];

        if (sourceItems is null || card is null || parentCardStock is null
            || cachedContainer is null) return;

        if (parentCardStock.Bounds == Rect.Empty)
        {
            return;
        }

        var cardIndex = sourceItems.IndexOf(card);
        var totalItems = sourceItems.Count;

        double totalOffset = 0;

        for (var i = 0; i <= cardIndex; i++)
        {
            if (i - 1 < 0) continue;

            var z = sourceItems[i - 1];
            GetOffsets(parentCardStock, z, i, totalItems, out var a, out var b);
            var c = z.IsFaceDown ? a : b;
            totalOffset += c;
        }

        if (totalOffset > 0)
        {
        }

        var startVector = parentCardStock.Orientation == Orientation.Horizontal
            ? new Vector((float) parentCardStock.Bounds.Left + (float) totalOffset, (float) parentCardStock.Bounds.Top)
            : new Vector((float) parentCardStock.Bounds.Left, (float) parentCardStock.Bounds.Top + (float) totalOffset);

        Canvas.SetLeft(cachedContainer, startVector.X);
        Canvas.SetTop(cachedContainer, startVector.Y);

        var cs = new ContainerState(new Point(), startVector, cachedContainer.ZIndex);
        ContainerStates[card] = cs;
    }


    private static void GetOffsets(CardStackControl parent, PlayingCardViewModel card, int n, int total,
        out double faceDownOffset,
        out double faceUpOffset)

    {
        faceDownOffset = 0;
        faceUpOffset = 0;

        //  We are now going to offset only if the offset mode is appropriate.
        switch (parent.OffsetMode)
        {
            case OffsetMode.EveryCard:
                //  Offset every card.
                faceDownOffset = parent.FaceDownOffset;
                faceUpOffset = parent.FaceUpOffset;
                break;
            case OffsetMode.EveryNthCard:
                //  Offset only if n Mod N is zero.
                if ((n + 1) % parent.NValue == 0)
                {
                    faceDownOffset = parent.FaceDownOffset;
                    faceUpOffset = parent.FaceUpOffset;
                }

                break;
            case OffsetMode.TopNCards:
                //  Offset only if (Total - N) <= n < Total
                if (n > (total - parent.NValue))
                {
                    faceDownOffset = parent.FaceDownOffset;
                    faceUpOffset = parent.FaceUpOffset;
                }

                break;

            case OffsetMode.BottomNCards:
                //  Offset only if 0 < n < N
                if (n < parent.NValue)
                {
                    faceDownOffset = parent.FaceDownOffset;
                    faceUpOffset = parent.FaceUpOffset;
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
    private static ImplicitAnimationCollection? ImplicitAnimations;


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