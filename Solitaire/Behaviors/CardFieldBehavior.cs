using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using CommunityToolkit.Mvvm.Input;
using Solitaire.Controls;
using Solitaire.ViewModels;

namespace Solitaire.Behaviors;

public sealed partial class CardFieldBehavior : Behavior<Canvas>, IDisposable
{
    [GeneratedAttachedProperty(Inherits = true)]
    public static partial List<CardStackPlacementControl> GetCardStacks(Control obj);


    private readonly Dictionary<PlayingCardViewModel, PlayingCard> _containerCache = new();
    private CardMotionController? _motion;
    private CardGameViewModel? _viewModel;
    private int _viewGeneration;
    private IPointer? _dragPointer;
    internal CardMotionController? Motion => _motion;
    internal int ViewGeneration => _viewGeneration;
    internal IPointer? DragPointer => _dragPointer;
    private readonly Dictionary<PlayingCard, CardStackPlacementControl> _sources = new();
    private readonly List<(INotifyCollectionChanged Source, NotifyCollectionChangedEventHandler Handler)> _stackSubscriptions = new();


    [GeneratedAttachedProperty]
    private static partial Vector? GetHomePosition(AvaloniaObject obj);

    /// <inheritdoc />
    protected override void OnAttached()
    {
        if (AssociatedObject == null)
            return;
        AssociatedObject.Background = Brushes.Transparent;
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

    private List<PlayingCard>? _draggingContainers;
    private List<PlayingCardViewModel>? _draggingCards;
    private bool _isDragging;
    private Point _startPoint;
    private List<int>? _startZIndices;
    private List<Vector>? _homePoints;
    private List<Vector>? _dragPoints;
    private CardStackPlacementControl? _homeStack;

    private void AssociatedObjectOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isDragging || _draggingContainers is null || _draggingCards is null)
            return;

        if (_motion is null || AssociatedObject is null || TopLevel.GetTopLevel(AssociatedObject) is not { } root)
            return;
        var absCur = e.GetCurrentPoint(root);
        var absCurPos = absCur.Position;

        foreach (var visual in root.GetVisualsAt(absCurPos)
                     .OrderByDescending(x => x.ZIndex))
        {
            if (visual is not CardStackPlacementControl { DataContext: CardGameViewModel game } toStack)
                continue;

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

                ResetDrag(!validMove);
            }

            break;
        }

        ResetDrag();
    }

    private void ResetDrag(bool returnHome = true)
    {
        if (!_isDragging || _draggingContainers is null || _draggingCards is null)
            return;

        foreach (var pair in _draggingContainers.Select((container, i) => (container, i)))
        {
            pair.container.Classes.Remove("dragging");

            if (!returnHome || _homePoints is null || _startZIndices is null)
                continue;

            _motion?.Move(pair.container, _homePoints[pair.i], false);
            pair.container.SetStackOrder(_startZIndices[pair.i]);
        }

        _motion?.Release();
        ClearDragState();
    }

    private void ClearDragState()
    {
        _isDragging = false;
        _draggingCards = null;
        _draggingContainers = null;
        _startZIndices = null;
        _startPoint = new Point();
        _dragPoints = null;
        _homePoints = null;
        _homeStack = null;
        var pointer = _dragPointer;
        _dragPointer = null;
        pointer?.Capture(null);
    }

    private void AssociatedObjectOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging || _draggingContainers is null || _draggingCards is null)
            return;

        if (!Equals(e.Pointer.Captured, _draggingContainers[0]))
            return;

        var position = e.GetCurrentPoint(_homeStack).Position;

        var delta = position - _startPoint;

        if (_dragPoints != null)
            _motion?.Drag(_draggingContainers, _dragPoints, delta);
    }

    private void AssociatedObjectOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_motion is null || AssociatedObject is null || TopLevel.GetTopLevel(AssociatedObject) is not { } root)
            return;
        var absCur = e.GetCurrentPoint(root);
        var absCurPos = absCur.Position;

        void ActivateCommand(CardStackPlacementControl? stack)
        {
            if (stack?.CommandOnCardClick?.CanExecute(null) ?? false)
            {
                stack.CommandOnCardClick?.Execute(null);
            }
        }

        if (!absCur.Properties.IsLeftButtonPressed)
            return;

        foreach (var visual in root.GetVisualsAt(absCurPos)
                     .OrderByDescending(x => x.ZIndex))
        {
            if (visual is CardStackPlacementControl { DataContext: CardGameViewModel } stack1)
            {
                ActivateCommand(stack1);
                break;
            }

            if (visual is not Border { DataContext: PlayingCardViewModel card } container)
                continue;

            var cardStacks = GetCardStacks(container);

            var stack2 =
                cardStacks.FirstOrDefault(x => x.SourceItems != null && x.SourceItems.Contains(card));

            if (stack2 is null)
                return;

            ActivateCommand(stack2);


            if (card.IsPlayable && !_isDragging)
            {
                _draggingContainers = new List<PlayingCard>();
                _draggingCards = new List<PlayingCardViewModel>();
                _startZIndices = new();
                _homePoints = new();
                _dragPoints = new();

                if (stack2.SourceItems != null)
                {
                    var cardIndex = stack2.SourceItems.IndexOf(card);

                    foreach (var c in stack2.SourceItems.Select((card2, i) => (card2, i))
                                 .Where(a => a.i >= cardIndex))
                    {
                        if (!_containerCache.TryGetValue(c.card2, out var cachedContainer))
                            continue;
                        _draggingContainers.Add(cachedContainer);
                        _draggingCards.Add(c.card2);
                        _startZIndices.Add(cachedContainer.StackOrder);
                        _homePoints.Add(GetHomePosition(cachedContainer) ?? throw new InvalidOperationException());
                        _dragPoints.Add(cachedContainer.Pose.Position);
                        cachedContainer.Classes.Add("dragging");

                    }

                    if (_draggingContainers.Count == 0)
                        return;
                }

                _isDragging = true;
                _motion?.Lift(_draggingContainers);

                _homeStack = stack2;
                _startPoint = e.GetCurrentPoint(_homeStack).Position;

                _dragPointer = e.Pointer;
                e.Pointer.Capture(_draggingContainers[0]);
            }

            break;
        }
    }

    protected override void OnDetachedFromVisualTree() => ResetView();

    public void ResetView()
    {
        ClearDragState();
        _motion?.Dispose();
        _motion = null;
        _sources.Clear();
        foreach (var card in _containerCache.Keys)
            card.PropertyChanged -= CardOnPropertyChanged;
        foreach (var (source, handler) in _stackSubscriptions)
            source.CollectionChanged -= handler;
        _stackSubscriptions.Clear();

        foreach (var container in _containerCache.Values)
        {
            container.ClearValue(HomePositionProperty);
            container.Content = null;
        }
        AssociatedObject?.Children.Clear();
        _containerCache.Clear();
        var model = _viewModel;
        _viewModel = null;
        model?.EndView(_viewGeneration);
    }

    public void Dispose()
    {
        ResetView();
        if (AssociatedObject != null)
        {
            AssociatedObject.PointerPressed -= AssociatedObjectOnPointerPressed;
            AssociatedObject.PointerMoved -= AssociatedObjectOnPointerMoved;
            AssociatedObject.PointerReleased -= AssociatedObjectOnPointerReleased;
            AssociatedObject.PointerCaptureLost -= AssociatedObjectOnPointerCaptureLost;
        }

    }

    protected override void OnDetaching()
    {
        Dispose();
        base.OnDetaching();
    }

    protected override void OnAttachedToVisualTree()
    {
        if (_motion is not null || AssociatedObject?.DataContext is not CardGameViewModel model)
            return;
        _viewModel = model;
        _viewGeneration = model.BeginView();

        _motion = new CardMotionController(AssociatedObject, RefreshContactShadows);

        var cardsList = model.Deck;
        var cardStacks = GetCardStacks(AssociatedObject);

        if (Application.Current == null ||
            !Application.Current.Styles.TryGetResource("PlayingCardDataTemplate", null, out var x) ||
            x is not DataTemplate y)
            return;

        if (!AssociatedObject.DataTemplates.Contains(y))
            AssociatedObject.DataTemplates.Add(y);

        var homePosition = cardStacks.FirstOrDefault(i => i.IsHomeStack)?.Bounds.Position ?? new Point();

        if (cardsList != null)
            foreach (var card in cardsList)
            {
                var container = new PlayingCard
                {
                    Content = card,
                    ZIndex = -1,
                    ClipToBounds = false
                };

                _containerCache.Add(card, container);
                _motion.Place(container, homePosition, card.IsFaceDown);
                card.PropertyChanged += CardOnPropertyChanged;
                AssociatedObject.Children.Add(container);
            }

        _motion.RefreshVisibility();

        foreach (var cardStack in cardStacks.Where(cardStack => cardStack.SourceItems != null))
        {
            if (cardStack.SourceItems is INotifyCollectionChanged source)
            {
                NotifyCollectionChangedEventHandler handler = (_, _) => UpdateStack(cardStack);
                source.CollectionChanged += handler;
                _stackSubscriptions.Add((source, handler));
            }
        }
    }

    private void UpdateStack(CardStackPlacementControl control)
    {
        if (control.SourceItems == null)
            return;

        // Removal also changes which cards form the visible waste fan.
        var sumOffsets = 0d;
        foreach (var pair in control.SourceItems.Select((card, i) => (card, i)))
        {
            if (!_containerCache.TryGetValue(pair.card, out var container))
                return;

            var pos = new Point(control.Bounds.Position.X +
                                (control.Orientation == Orientation.Horizontal ? sumOffsets : 0),
                control.Bounds.Position.Y + (control.Orientation == Orientation.Vertical ? sumOffsets : 0));
            GetOffsets(control, pair.card, pair.i, control.SourceItems.Count, out var faceDownOffset, out var faceUpOffset);
            sumOffsets += pair.card.IsFaceDown ? faceDownOffset : faceUpOffset;

            container.Classes.Remove("lastCard");
            if (pair.i == control.SourceItems.Count - 1 || pair.i == control.SourceItems.Count - 2)
            {
                container.Classes.Add("lastCard");
            }

            container.SetStackOrder(pair.i);
            container.Classes.Add("playingCard");

            _sources.TryGetValue(container, out var source);
            var opening = control.Classes.Contains("RunMarker") &&
                pair.card.CardGameInstance?.NewGameCommand is IAsyncRelayCommand { IsRunning: true };
            var deal = opening || source?.CommandOnCardClick != null && source != control;
            _sources[container] = control;
            SetHomePosition(container, pos);
            _motion?.Move(container, pos, deal);
            _motion?.Reveal(container, pair.card.IsFaceDown);
        }
        RefreshContactShadows();
    }

    private void CardOnPropertyChanged(object? sender, PropertyChangedEventArgs change)
    {
        if (change.PropertyName == nameof(PlayingCardViewModel.IsFaceDown) &&
            sender is PlayingCardViewModel card && _containerCache.TryGetValue(card, out var container) &&
            card.CardGameInstance?.GetCardCollection(card) != null)
            _motion?.Reveal(container, card.IsFaceDown);
    }

    private void RefreshContactShadows()
    {
        if (AssociatedObject is null)
            return;
        _motion?.RefreshVisibility();
        foreach (var stack in GetCardStacks(AssociatedObject))
        {
            if (stack.SourceItems is not { } cards)
                continue;
            for (var i = 0; i < cards.Count; i++)
            {
                if (!_containerCache.TryGetValue(cards[i], out var card))
                    continue;
                var covered = i + 1 < cards.Count &&
                    _containerCache.TryGetValue(cards[i + 1], out var next) &&
                    !next.IsFloating && GetHomePosition(card) == GetHomePosition(next);
                card.SetContactExposed(!covered);
            }
        }
    }

    private static void GetOffsets(CardStackPlacementControl parent, PlayingCardViewModel card, int n, int total,
        out double faceDownOffset,
        out double faceUpOffset)

    {
        faceDownOffset = 0;
        faceUpOffset = 0;

        switch (parent.OffsetMode)
        {
            case OffsetMode.EveryCard:
                faceDownOffset = parent.FaceDownOffset ?? default;
                faceUpOffset = parent.FaceUpOffset ?? default;
                break;
            case OffsetMode.EveryNthCard:
                if ((n + 1) % (int)parent.NValue == 0)
                {
                    faceDownOffset = parent.FaceDownOffset ?? default;
                    faceUpOffset = parent.FaceUpOffset ?? default;
                }

                break;


            case OffsetMode.TopNCards:
                var k = (int)parent.NValue;

                if ((total - k) <= n && n < total)
                {
                    faceDownOffset = parent.FaceDownOffset ?? default;
                    faceUpOffset = parent.FaceUpOffset ?? default;
                }

                break;

            case OffsetMode.BottomNCards:
                if (n <= (int)(parent.NValue))
                {
                    faceDownOffset = parent.FaceDownOffset ?? default;
                    faceUpOffset = parent.FaceUpOffset ?? default;
                }

                break;
            case OffsetMode.UseCardValues:
                faceDownOffset = card.FaceDownOffset;
                faceUpOffset = card.FaceUpOffset;
                break;
        }
    }
}
