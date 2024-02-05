using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using Solitaire.Controls;
using Solitaire.ViewModels;

namespace Solitaire.Behaviors;

public class CardFieldBehavior : Behavior<Canvas>
{
    public static readonly AttachedProperty<List<CardStackPlacementControl>> CardStacksProperty =
        AvaloniaProperty.RegisterAttached<CardFieldBehavior, Control, List<CardStackPlacementControl>>(
            "CardStacks", inherits: true);

    public static void SetCardStacks(Control obj, List<CardStackPlacementControl> value) =>
        obj.SetValue(CardStacksProperty, value);

    public static List<CardStackPlacementControl> GetCardStacks(Control obj) => obj.GetValue(CardStacksProperty);


    private readonly Dictionary<PlayingCardViewModel, PlayingCard> _containerCache = new();


    private static readonly AttachedProperty<Vector?> HomePositionProperty =
        AvaloniaProperty.RegisterAttached<CardFieldBehavior, AvaloniaObject, Vector?>(
            "HomePosition");

    private static void SetHomePosition(AvaloniaObject obj, Vector? value) =>
        obj.SetValue(HomePositionProperty, value);

    private static Vector? GetHomePosition(AvaloniaObject obj) => obj.GetValue(HomePositionProperty);

    /// <inheritdoc />
    protected override void OnAttached()
    {
        if (AssociatedObject == null) return;
        AssociatedObject.Background = Brushes.Transparent;
        AssociatedObject.AttachedToVisualTree += AssociatedObjectOnAttachedToVisualTree;
        AssociatedObject.DetachedFromVisualTree += AssociatedObjectOnDetachedFromVisualTree;
        AssociatedObject.KeyDown += AssociatedObjectOnKeyDown;
        AssociatedObject.PointerPressed += AssociatedObjectOnPointerPressed;
        AssociatedObject.PointerMoved += AssociatedObjectOnPointerMoved;
        AssociatedObject.PointerReleased += AssociatedObjectOnPointerReleased;
        AssociatedObject.PointerCaptureLost += AssociatedObjectOnPointerCaptureLost;
        base.OnAttached();
    }

    private void AssociatedObjectOnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        ResetDragAndKeyMove();
    }

    private List<Control>? _draggingContainers, _keyMoveContainers;
    private List<PlayingCardViewModel>? _draggingCards, _keyboardMoveCards;
    private bool _isDragging, _keyboardMove;
    private Point _startPoint;
    private List<int>? _startZIndices;
    private List<Vector>? _homePoints;
    private CardStackPlacementControl? _homeStack;

    private void AssociatedObjectOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isDragging || _draggingContainers is null || _draggingCards is null) return;

        var absCur = e.GetCurrentPoint(TopLevel.GetTopLevel(AssociatedObject));
        var absCurPos = absCur.Position;

        if (AssociatedObject is null) return;

        foreach (var visual in TopLevel.GetTopLevel(AssociatedObject)!.GetVisualsAt(absCurPos)
                     .OrderByDescending(x => x.ZIndex))
        {
            if (visual is not CardStackPlacementControl { DataContext: CardGameViewModel game } toStack) continue;

            var cardStacks = GetCardStacks(_draggingContainers![0]);
            var fromStack =
                cardStacks.FirstOrDefault(x => x.SourceItems != null && x.SourceItems.Contains(_draggingCards[0]));

            // Trigger on different stack.
            if (fromStack?.SourceItems != null && toStack.SourceItems != null &&
                !fromStack.SourceItems.SequenceEqual(toStack.SourceItems))
            {
                // Save reference to current card before resetting. 
                var targetCard = _draggingCards[0];
                var validMove = game.CheckAndMoveCard(fromStack.SourceItems, toStack.SourceItems, targetCard);

                ResetDragAndKeyMove(!validMove);
            }

            break;
        }

        ResetDragAndKeyMove();
    }

    private bool ResetDragAndKeyMove(bool returnHome = true)
    {
        if (!_isDragging && !_keyboardMove) return false;

        if (_draggingContainers is not null)
        {
            foreach (var pair in _draggingContainers.Select((container, i) => (container, i)))
            {
                pair.container.Classes.Remove("dragging");

                if (!returnHome || _homePoints is null || _startZIndices is null) continue;

                SetCanvasPosition(pair.container, _homePoints[pair.i]);
                pair.container.ZIndex = _startZIndices[pair.i];
            }
        }
        
        foreach (var cardStack in GetCardStacks(AssociatedObject!))
        {
            cardStack.ClearValue(InputElement.FocusableProperty);
        }
        
        AssociatedObject!.IsEnabled = true;

        _keyMoveContainers?.LastOrDefault()?.Focus(NavigationMethod.Directional);

        _isDragging = false;
        _draggingCards = null;
        _draggingContainers = null;
        _startZIndices = null;
        _startPoint = new Point();

        _keyboardMove = false;
        _keyboardMoveCards = null;
        _keyMoveContainers = null;
        
        return true;
    }

    private void AssociatedObjectOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging || _draggingContainers is null || _draggingCards is null) return;

        if (!Equals(e.Pointer.Captured, _draggingContainers[0])) return;

        var position = e.GetCurrentPoint(_homeStack).Position;

        var delta = position - _startPoint;

        foreach (var draggingContainer in _draggingContainers.Select((control, i) => (control, i)))
        {
            if (_homePoints == null) continue;
            SetCanvasPosition(draggingContainer.control, _homePoints[draggingContainer.i] + delta);
        }
    }

    private static void SetCanvasPosition(AvaloniaObject? control, Vector newVector)
    {
        if (control is null) return;

        Canvas.SetLeft(control, newVector.X);
        Canvas.SetTop(control, newVector.Y);
    }

    private void AssociatedObjectOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (_isDragging) return;

        var focusedCardView = ((Control?)e.Source)?.FindAncestorOfType<PlayingCard>(true);
        var focusedPlacement = ((Control?)e.Source)?.FindAncestorOfType<CardStackPlacementControl>(true)
            ?? TopLevel.GetTopLevel(AssociatedObject)?.FocusManager!.GetFocusedElement() as CardStackPlacementControl;

        if (e.Key == Key.Space && focusedCardView is not null)
        {
            if (GetStackAndIndex(focusedCardView) is { } tuple)
            {
                _keyboardMoveCards = new List<PlayingCardViewModel>();
                _keyMoveContainers = new List<Control>();

                foreach (var c in tuple.stack.SourceItems!.Select((card2, i) => (card2, i))
                             .Where(a => a.i >= tuple.currentIndex))
                {
                    if (!_containerCache.TryGetValue(c.card2, out var cachedContainer)) continue;
                    _keyMoveContainers.Add(cachedContainer);
                    _keyboardMoveCards.Add(c.card2);
                    // _startZIndices.Add(cachedContainer.ZIndex);
                    // _homePoints.Add(GetHomePosition(cachedContainer) ?? throw new InvalidOperationException());
                    // cachedContainer.Classes.Add("dragging");
                    cachedContainer.ZIndex = int.MaxValue / 2 + c.i;
                }

                if (_keyMoveContainers.Any())
                {
                    e.Handled = _keyboardMove = true;
                    
                    foreach (var cardStack in GetCardStacks(AssociatedObject!))
                    {
                        cardStack.SetCurrentValue(InputElement.FocusableProperty, true);
                    }
                    AssociatedObject!.IsEnabled = false;

                    tuple.stack.Focus(NavigationMethod.Directional);
                }
            }
        }
        else if (e.Key == Key.Space && focusedPlacement is not null)
        {
            if (_keyboardMove && _keyboardMoveCards is not null)
            {
                var game = (CardGameViewModel)focusedPlacement.DataContext!;

                var cardStacks = GetCardStacks(_keyMoveContainers![0]);
                var fromStack = cardStacks.FirstOrDefault(x =>
                    x.SourceItems != null && x.SourceItems.Contains(_keyboardMoveCards[0]));

                if (fromStack?.SourceItems != null && focusedPlacement.SourceItems != null &&
                    !fromStack.SourceItems.SequenceEqual(focusedPlacement.SourceItems))
                {
                    // Save reference to current card before resetting. 
                    var targetCard = _keyboardMoveCards[0];
                    var validMove =
                        game.CheckAndMoveCard(fromStack.SourceItems, focusedPlacement.SourceItems, targetCard);

                    e.Handled = ResetDragAndKeyMove(!validMove);
                }
                else
                {
                    e.Handled = ResetDragAndKeyMove();
                }
            }
            else if (focusedPlacement.CommandOnCardClick?.CanExecute(null) ?? false)
            {
                focusedPlacement.CommandOnCardClick.Execute(null);
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Escape && _keyboardMove)
        {
            e.Handled = ResetDragAndKeyMove();
        }
        else if (e.Key is Key.Up or Key.Down && focusedCardView is not null)
        {
            if (GetStackAndIndex(focusedCardView) is { } tuple)
            {
                var newIndex = tuple.currentIndex + (e.Key is Key.Up ? -1 : 1);
                if (tuple.stack.SourceItems!.Skip(newIndex).FirstOrDefault() is {} newCard
                    && _containerCache.TryGetValue(newCard, out var cachedContainer))
                {
                    e.Handled = cachedContainer.Focus(NavigationMethod.Directional);
                }
            }
        }
        
        static (CardStackPlacementControl stack, int currentIndex)? GetStackAndIndex(PlayingCard sourceCardView)
        {
            var card = (PlayingCardViewModel)sourceCardView.DataContext!;
            var cardStacks = GetCardStacks(sourceCardView);
            var stack = cardStacks.FirstOrDefault(x => x.SourceItems != null && x.SourceItems.Contains(card));
            var currentIndex = stack?.SourceItems!.IndexOf(card);
            if (currentIndex.HasValue)
            {
                return (stack!, currentIndex.Value);
            }
            return null;
        }
    }

    private void AssociatedObjectOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_keyboardMove)
        {
            ResetDragAndKeyMove();
        }

        var absCur = e.GetCurrentPoint(TopLevel.GetTopLevel(AssociatedObject));
        var absCurPos = absCur.Position;

        void ActivateCommand(CardStackPlacementControl? stack)
        {
            if (stack?.CommandOnCardClick?.CanExecute(null) ?? false)
            {
                stack.CommandOnCardClick?.Execute(null);
            }
        }

        if (!absCur.Properties.IsLeftButtonPressed || AssociatedObject == null) return;

        foreach (var visual in TopLevel.GetTopLevel(AssociatedObject)!.GetVisualsAt(absCurPos)
                     .OrderByDescending(x => x.ZIndex))
        {
            if (visual is CardStackPlacementControl { DataContext: CardGameViewModel } stack1)
            {
                ActivateCommand(stack1);
                break;
            }

            if (visual is not Border { DataContext: PlayingCardViewModel card } container) continue;

            var cardStacks = GetCardStacks(container);

            var stack2 =
                cardStacks.FirstOrDefault(x => x.SourceItems != null && x.SourceItems.Contains(card));

            if (stack2 is null) return;

            ActivateCommand(stack2);


            if (card.IsPlayable && !_isDragging)
            {
                _draggingContainers = new List<Control>();
                _draggingCards = new List<PlayingCardViewModel>();
                _startZIndices = new();
                _homePoints = new();

                if (stack2.SourceItems != null)
                {
                    var cardIndex = stack2.SourceItems.IndexOf(card);

                    foreach (var c in stack2.SourceItems.Select((card2, i) => (card2, i))
                                 .Where(a => a.i >= cardIndex))
                    {
                        if (!_containerCache.TryGetValue(c.card2, out var cachedContainer)) continue;
                        _draggingContainers.Add(cachedContainer);
                        _draggingCards.Add(c.card2);
                        _startZIndices.Add(cachedContainer.ZIndex);
                        _homePoints.Add(GetHomePosition(cachedContainer) ?? throw new InvalidOperationException());
                        cachedContainer.Classes.Add("dragging");
                        cachedContainer.ZIndex = int.MaxValue / 2 + c.i;
                    }

                    if (_draggingContainers.Count == 0) return;
                }

                _isDragging = true;

                _homeStack = stack2;
                _startPoint = e.GetCurrentPoint(_homeStack).Position;

                e.Pointer.Capture(_draggingContainers[0]);
            }

            break;
        }
    }

    private void AssociatedObjectOnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Canvas s)
        {
            var cardStacks = GetCardStacks(s);
            cardStacks.Clear();
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

        var cardsList = model.Deck;
        var cardStacks = GetCardStacks(AssociatedObject);

        var homePosition = cardStacks.FirstOrDefault(i => i.IsHomeStack)?.Bounds.Position ?? new Point();

        if (cardsList != null)
            foreach (var card in cardsList)
            {
                var container = new PlayingCard
                {
                    DataContext = card,
                    ZIndex = -1,
                    ClipToBounds = false
                };

                _containerCache.Add(card, container);
                AssociatedObject.Children.Add(container);

                SetCanvasPosition(container, homePosition);
            }

        foreach (var cardStack in cardStacks.Where(cardStack => cardStack.SourceItems != null))
        {
            if (cardStack.SourceItems != null)
                (cardStack.SourceItems as INotifyCollectionChanged).CollectionChanged +=
                    delegate(object? _, NotifyCollectionChangedEventArgs args)
                    {
                        SourceItemsOnCollectionChanged(cardStack, args);
                    };
        }
    }

    private void SourceItemsOnCollectionChanged(CardStackPlacementControl? control, NotifyCollectionChangedEventArgs e)
    {
        if (control is null || e.Action != NotifyCollectionChangedAction.Add || e.NewItems == null)
        {
            return;
        }

        if (control.SourceItems == null) return;


        foreach (var pair in control.SourceItems.Select((card, i) => (card, i)))
        {
            if (!_containerCache.TryGetValue(pair.card, out var container)) return;

            var sumOffsets = control.SourceItems
                .Select((card, i) => (card, i))
                .Where(a => a.i < pair.i)
                .Select(b =>
                {
                    GetOffsets(control, b.card, b.i, control.SourceItems.Count, out var c,
                        out var d);

                    return b.card.IsFaceDown ? c : d;
                })
                .Sum();


            var pos = new Point(control.Bounds.Position.X +
                                (control.Orientation == Orientation.Horizontal ? sumOffsets : 0),
                control.Bounds.Position.Y + (control.Orientation == Orientation.Vertical ? sumOffsets : 0));

            var isLastCard = pair.i == control.SourceItems.Count - 1 || pair.i == control.SourceItems.Count - 2;
            container.Classes.Set("lastCard", isLastCard);

            container.ZIndex = pair.i;

            SetHomePosition(container, pos);
            SetCanvasPosition(container, pos);
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
                if ((n + 1) % (int)parent.NValue == 0)
                {
                    faceDownOffset = parent.FaceDownOffset ?? default;
                    faceUpOffset = parent.FaceUpOffset ?? default;
                }

                break;


            case OffsetMode.TopNCards:
                //  Offset only if (Total - N) <= n < Total
                var k = (int)parent.NValue;

                if ((total - k) <= n && n < total)
                {
                    faceDownOffset = parent.FaceDownOffset ?? default;
                    faceUpOffset = parent.FaceUpOffset ?? default;
                }

                break;

            case OffsetMode.BottomNCards:
                //  Offset only if 0 < n < N
                if (n <= (int)(parent.NValue))
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
}