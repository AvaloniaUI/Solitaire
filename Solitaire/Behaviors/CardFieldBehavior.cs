using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using Solitaire.Controls;
using Solitaire.ViewModels;

namespace Solitaire.Behaviors;

public class CardFieldBehavior : Behavior<Canvas>
{
    private static ImplicitAnimationCollection? ImplicitAnimations;

    public static readonly AttachedProperty<List<CardStackPlacementControl>> CardStacksProperty =
        AvaloniaProperty.RegisterAttached<CardFieldBehavior, Control, List<CardStackPlacementControl>>(
            "CardStacks", inherits: true);

    public List<PlayingCardViewModel> Cards
    {
        get => GetValue(CardsProperty);
        set => SetValue(CardsProperty, value);
    }

    public static void SetCardStacks(Control obj, List<CardStackPlacementControl> value) =>
        obj.SetValue(CardStacksProperty, value);

    public static List<CardStackPlacementControl>? GetCardStacks(Control obj) => obj.GetValue(CardStacksProperty);


    private readonly Dictionary<PlayingCardViewModel, ContentControl> _containerCache = new();

    public static readonly StyledProperty<List<PlayingCardViewModel>> CardsProperty =
        AvaloniaProperty.Register<CardFieldBehavior, List<PlayingCardViewModel>>("Cards");


    private void EnsureImplicitAnimations()
    {
        if (ImplicitAnimations != null || AssociatedObject == null) return;


        var x = AssociatedObject.GetVisualParent() as Visual;
        var u = ElementComposition.GetElementVisual(x);
        var compositor = u.Compositor;

        var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.Target = "Offset";
        offsetAnimation.InsertExpressionKeyFrame(0.0f, "this.InitialValue");
        offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        offsetAnimation.Duration = TimeSpan.FromMilliseconds(500);

        var animationGroup = compositor.CreateAnimationGroup();
        animationGroup.Add(offsetAnimation);

        ImplicitAnimations = compositor.CreateImplicitAnimationCollection();
        ImplicitAnimations["Offset"] = animationGroup;
    }


    /// <inheritdoc />
    protected override void OnAttached()
    {
        if (AssociatedObject == null) return;
        AssociatedObject.Background = Brushes.Transparent;
        AssociatedObject.AttachedToVisualTree += AssociatedObjectOnAttachedToVisualTree;
        AssociatedObject.DetachedFromVisualTree += AssociatedObjectOnDetachedFromVisualTree;
        AssociatedObject.PointerPressed += AssociatedObjectOnPointerPressed;
        AssociatedObject.PointerMoved += AssociatedObjectOnPointerMoved;
        AssociatedObject.PointerReleased += AssociatedObjectOnPointerReleased;
        AssociatedObject.PointerCaptureLost += AssociatedObjectOnPointerCaptureLost;
        base.OnAttached();
    }

    private void AssociatedObjectOnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        ResetDrag();
    }

    private Control? _draggingContainer;
    private PlayingCardViewModel? _draggingCard;
    private bool _isDragging;
    private Point _startPoint;
    private int _startZIndex;

    private void AssociatedObjectOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isDragging || _draggingContainer is null || _draggingCard is null) return;

        var absCur = e.GetCurrentPoint(AssociatedObject?.GetVisualRoot());
        var absCurPos = absCur.Position;

        if (AssociatedObject is null) return;

        foreach (var visual in AssociatedObject.GetVisualRoot()!.GetVisualsAt(absCurPos)
                     .OrderByDescending(x => x.ZIndex))
        {
            if (visual is not CardStackPlacementControl {DataContext: CardGameViewModel game} toStack) continue;

            var cardStacks = GetCardStacks(_draggingContainer);
            var fromStack =
                cardStacks?.FirstOrDefault(x => x.SourceItems != null && x.SourceItems.Contains(_draggingCard));

            // Trigger on different stack.
            if (fromStack?.SourceItems != null && toStack?.SourceItems != null &&
                !fromStack.SourceItems.SequenceEqual(toStack.SourceItems))
            {
                // Save reference to current card before resetting. 
                var targetCard = _draggingCard;
                ResetDrag();
                game.CheckAndMoveCard(fromStack.SourceItems, toStack.SourceItems, targetCard);

            }
            break;
        }

        ResetDrag();
    }

    private void ResetDrag()
    {
        if (!_isDragging || _draggingContainer is null || _draggingCard is null) return;

       // ((IPseudoClasses) _draggingContainer.Classes).Remove(".dragging");
        SetTranslateTransform(_draggingContainer, Vector.Zero);
        _draggingContainer.ZIndex = _startZIndex;
        _isDragging = false;
        _draggingCard = null;
        _draggingContainer = null;
        _startPoint = new Point();
    }

    private void AssociatedObjectOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging || _draggingContainer is null || _draggingCard is null) return;

        if (!Equals(e.Pointer.Captured, _draggingContainer)) return;

        var position = e.GetCurrentPoint(_draggingContainer.Parent).Position;

        var delta = position - _startPoint;

        SetTranslateTransform(_draggingContainer, delta);
    }

    private static void SetTranslateTransform(IControl? control, Vector newVector)
    {
        if (control is null) return;
        var transformBuilder = new TransformOperations.Builder(1);
        transformBuilder.AppendTranslate(newVector.X, newVector.Y);
        control.SetValue(Visual.RenderTransformProperty, transformBuilder.Build(), BindingPriority.Style);
        // control.RenderTransform = transformBuilder.Build();
    }

    private void AssociatedObjectOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var absCur = e.GetCurrentPoint(AssociatedObject?.GetVisualRoot());
        var absCurPos = absCur.Position;

        void ActivateCommand(CardStackPlacementControl? stack)
        {
            if (stack?.CommandOnCardClick?.CanExecute(null) ?? false)
            {
                stack.CommandOnCardClick?.Execute(null);
            }
        }

        if (!absCur.Properties.IsLeftButtonPressed || AssociatedObject == null) return;

        foreach (var visual in AssociatedObject.GetVisualRoot()!.GetVisualsAt(absCurPos)
                     .OrderByDescending(x => x.ZIndex))
        {
            if (visual is CardStackPlacementControl {DataContext: CardGameViewModel a} stack1)
            {
                ActivateCommand(stack1);
                break;
            }

            if (visual is not Border {DataContext: PlayingCardViewModel card} container) continue;
            
            var cardStacks = GetCardStacks(container);
            if (cardStacks != null)
            {
                var stack2 =
                    cardStacks.FirstOrDefault(x => x.SourceItems != null && x.SourceItems.Contains(card));
                ActivateCommand(stack2);
            }

            if (card.IsPlayable && !_isDragging && _containerCache.TryGetValue(card, out var cachedContainer))
            {
                _isDragging = true;
                _draggingContainer = cachedContainer;
                _draggingCard = card;

                // ((IPseudoClasses) cachedContainer.Classes).Add(".dragging");

                _startPoint = e.GetCurrentPoint(cachedContainer!.Parent).Position;
                _startZIndex = _draggingContainer.ZIndex;
                _draggingContainer.ZIndex = Int32.MaxValue;

                e.Pointer.Capture(cachedContainer);
                break;
            }

            break;
        }
    }


    private void AssociatedObjectOnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Canvas s)
        {
            var cardStacks = GetCardStacks(s);
            cardStacks?.Clear();
            s.Children.Clear();
        }


        if (AssociatedObject != null)
        {
            AssociatedObject.AttachedToVisualTree -= AssociatedObjectOnAttachedToVisualTree;
            AssociatedObject.DetachedFromVisualTree -= AssociatedObjectOnDetachedFromVisualTree;
            AssociatedObject.PointerPressed -= AssociatedObjectOnPointerPressed;
            AssociatedObject.PointerMoved -= AssociatedObjectOnPointerMoved;
            AssociatedObject.PointerReleased -= AssociatedObjectOnPointerReleased;
            AssociatedObject.PointerCaptureLost -= AssociatedObjectOnPointerCaptureLost;
        }

        _containerCache.Clear();
    }

    private void AssociatedObjectOnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (AssociatedObject?.DataContext is not CardGameViewModel model) return;

        //    EnsureImplicitAnimations();

        var cardsList = model.PlayingCards;
        var cardStacks = GetCardStacks(AssociatedObject);

        if (Application.Current == null ||
            !Application.Current.Styles.TryGetResource("PlayingCardDataTemplate", out var x) ||
            x is not DataTemplate y) return;

        AssociatedObject.DataTemplates.Add(y);

        var homePosition = cardStacks?.FirstOrDefault(i => i.IsHomeStack)?.Bounds.Position ?? new Point();

        foreach (var card in cardsList)
        {
            var container = new ContentControl
            {
                Content = card,
                ZIndex = -1
            };

            container.Classes.Add("playingCard");
            _containerCache.Add(card, container);
            AssociatedObject.Children.Add(container);

            Canvas.SetLeft(container, homePosition.X);
            Canvas.SetTop(container, homePosition.Y);
        }

        if (cardStacks == null) return;
        foreach (var cardStack in cardStacks.Where(cardStack => cardStack.SourceItems != null))
        {
            if (cardStack.SourceItems != null)
                cardStack.SourceItems.CollectionChanged +=
                    delegate(object? _, NotifyCollectionChangedEventArgs args)
                    {
                        SourceItemsOnCollectionChanged(cardStack, args);
                    };
        }
    }

    private void SourceItemsOnCollectionChanged(CardStackPlacementControl? control, NotifyCollectionChangedEventArgs e)
    {
        if (control is null || e.Action != NotifyCollectionChangedAction.Add || e.NewItems == null ||
            e.NewItems.Count > 1)
        {
            return;
        }

        if (e.NewItems[0] is not PlayingCardViewModel newItem ||
            !_containerCache.TryGetValue(newItem, out var container)) return;

        if (control.SourceItems == null) return;

        var index = e.NewStartingIndex;

        var sumOffsets = control.SourceItems
            .Select((card, i) => (card, i))
            .Where(tuple => tuple.i < index)
            .Select(z =>
            {
                GetOffsets(control, z.card, z.i, control.SourceItems.Count, out var xx,
                    out var yy);

                return z.card.IsFaceDown ? xx : yy;
            })
            .Sum();

        var pos = new Point(control.Bounds.Position.X +
                            (control.Orientation == Orientation.Horizontal ? sumOffsets : 0),
            control.Bounds.Position.Y + (control.Orientation == Orientation.Vertical ? sumOffsets : 0));

        container.ZIndex = index;
        Canvas.SetLeft(container, pos.X);
        Canvas.SetTop(container, pos.Y);
    }

    private static void AddImplicitAnimations(Visual container)
    {
        if (ElementComposition.GetElementVisual(container) is { } compositionVisual)
        {
            compositionVisual.ImplicitAnimations = ImplicitAnimations;
        }
    }


    private static void GetOffsets(CardStackPlacementControl parent, PlayingCardViewModel card, int n, int total,
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
                faceDownOffset = parent.FaceDownOffset ?? default;
                faceUpOffset = parent.FaceUpOffset ?? default;
                break;
            case OffsetMode.EveryNthCard:
                //  Offset only if n Mod N is zero.
                if ((n + 1) % (int) parent.NValue == 0)
                {
                    faceDownOffset = parent.FaceDownOffset ?? default;
                    faceUpOffset = parent.FaceUpOffset ?? default;
                }

                break;


            case OffsetMode.TopNCards:
                //  Offset only if (Total - N) <= n < Total
                var k = (int) parent.NValue;

                if ((total - k) <= n && n < total)
                {
                    faceDownOffset = parent.FaceDownOffset ?? default;
                    faceUpOffset = parent.FaceUpOffset ?? default;
                }

                break;

            case OffsetMode.BottomNCards:
                //  Offset only if 0 < n < N
                if (n <= (int) (parent.NValue))
                {
                    faceDownOffset = parent.FaceDownOffset ?? default;
                    faceUpOffset = parent.FaceUpOffset ?? default;
                }

                break;
            case OffsetMode.UseCardValues:
                //  Offset each time by the amount specified in the card object.
                faceDownOffset = card.FaceDownOffset;
                faceUpOffset = card.FaceUpOffset;
                break;
        }
    }

    private static void RemoveImplicitAnimations(Visual container)
    {
        if (ElementComposition.GetElementVisual(container) is { } compositionVisual)
        {
            compositionVisual.ImplicitAnimations = null;
        }
    }
}