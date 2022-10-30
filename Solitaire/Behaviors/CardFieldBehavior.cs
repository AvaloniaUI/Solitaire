using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using Microsoft.CodeAnalysis.Operations;
using Solitaire.Controls;
using Solitaire.Models;
using Solitaire.ViewModels;
using Solitaire.ViewModels.Pages;

namespace Solitaire.Behaviors;

public class CardFieldBehavior : Behavior<Canvas>
{
    private static ImplicitAnimationCollection? ImplicitAnimations;

    public static readonly AttachedProperty<List<CardStackPlacementControl>> CardStacksProperty =
        AvaloniaProperty.RegisterAttached<CardFieldBehavior, Control, List<CardStackPlacementControl>>(
            "CardStacks", inherits: true);

    public static void SetCardStacks(Control obj, List<CardStackPlacementControl> value) =>
        obj.SetValue(CardStacksProperty, value);

    public static List<CardStackPlacementControl>? GetCardStacks(Control obj) => obj.GetValue(CardStacksProperty);


    public static readonly AttachedProperty<List<PlayingCardViewModel>> CardsProperty =
        AvaloniaProperty.RegisterAttached<CardFieldBehavior, Control, List<PlayingCardViewModel>>("Cards");

    private Dictionary<CardType, ContentControl> _containerCache = new();
    private bool _isLayouting;

    public static void SetCards(Control obj, List<PlayingCardViewModel> value) => obj.SetValue(CardsProperty, value);
    public static List<PlayingCardViewModel> GetCards(Control obj) => obj.GetValue(CardsProperty);

    private void EnsureImplicitAnimations()
    {
        if (ImplicitAnimations != null || AssociatedObject == null) return;


        var x = AssociatedObject.GetVisualRoot() as Visual;
        var u = ElementComposition.GetElementVisual(x);
        var compositor = u.Compositor;

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
    protected override void OnAttached()
    {
        if (AssociatedObject == null) return;
        AssociatedObject.Loaded += AssociatedObjectOnLoaded;
        AssociatedObject.Unloaded += AssociatedObjectOnUnloaded;
        base.OnAttached();
    }

    private void AssociatedObjectOnUnloaded(object? sender, RoutedEventArgs e)
    {
        var s = sender as Canvas;
        var cardsList = GetCards(s);
        var cardStacks = GetCardStacks(s);

        if (cardStacks != null)
        {
            cardsList.Clear();
            cardStacks.Clear();
        }

        _containerCache.Clear();
        s.Children.Clear();
    }

    private void AssociatedObjectOnLoaded(object? sender, RoutedEventArgs e)
    {
        if (AssociatedObject == null) return;

        EnsureImplicitAnimations();

        var cardsList = GetCards(AssociatedObject);
        var cardStacks = GetCardStacks(AssociatedObject);

        if (cardStacks != null)
        {
        }

        if (Application.Current == null ||
            !Application.Current.Styles.TryGetResource("PlayingCardDataTemplate", out var x) ||
            x is not DataTemplate y) return;

        AssociatedObject.DataTemplates.Add(y);

        var homePosition = cardStacks?.FirstOrDefault(i => i.IsHomeStack)?.Bounds.Position ?? new Point();

        foreach (var cardType in Enum.GetValuesAsUnderlyingType(typeof(CardType)).Cast<CardType>())
        {
            var card = new PlayingCardViewModel(AssociatedObject.DataContext as CardGameViewModel)
            {
                CardType = cardType,
                IsFaceDown = true
            };

            var container = new ContentControl
            {
                Content = card
            };

            _containerCache.Add(cardType, container);

            Canvas.SetLeft(container, homePosition.X);
            Canvas.SetTop(container, homePosition.Y);

            AssociatedObject.Children.Add(container);
            cardsList.Add(card);
            container.Loaded += ContainerOnLoaded;
        }


        foreach (var cardStack in cardStacks)
        {
            if (cardStack.SourceItems != null)
                cardStack.SourceItems.CollectionChanged += delegate(object? o, NotifyCollectionChangedEventArgs args)
                {
                    if (o is not ObservableCollection<PlayingCardViewModel> col) return;
                    SourceItemsOnCollectionChanged(cardStack, col, args);
                };
        }
        // LayoutCards();
    }

    private void SourceItemsOnCollectionChanged(CardStackPlacementControl control,
        ObservableCollection<PlayingCardViewModel> col, NotifyCollectionChangedEventArgs e)
    {
        foreach (var ca in col.Select(x => (col.IndexOf(x), x)).OrderBy(x => x.Item1))
        {
            if (!_containerCache.TryGetValue(ca.x.CardType, out var container)) continue;

            container.ZIndex = ca.Item1;

            GetOffsets(control, ca.x, ca.Item1, col.Count, out var xx,
                out var yy);

            var totalOffset = (ca.x.IsFaceDown ? xx : yy) * ca.Item1;

            var pos = new Point(control.Bounds.Position.X +
                                (control.Orientation == Orientation.Horizontal ? totalOffset : 0)
                , control.Bounds.Position.Y
                  + (control.Orientation == Orientation.Vertical ? totalOffset : 0)
            );

            Canvas.SetLeft(container, pos.X);
            Canvas.SetTop(container, pos.Y);
        }
    }

    private void ContainerOnLoaded(object? sender, RoutedEventArgs e)
    {
        AddImplicitAnimations(sender as ContentControl ?? throw new InvalidOperationException());
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
                if ((n + 1) % ((int) parent.NValue) == 0)
                {
                    faceDownOffset = parent.FaceDownOffset ?? default;
                    faceUpOffset = parent.FaceUpOffset ?? default;
                }

                break;
            case OffsetMode.TopNCards:
                //  Offset only if (Total - N) <= n < Total
                if (n > (total - (int) parent.NValue))
                {
                    faceDownOffset = parent.FaceDownOffset ?? default;
                    faceUpOffset = parent.FaceUpOffset ?? default;
                }

                break;

            case OffsetMode.BottomNCards:
                //  Offset only if 0 < n < N
                if (n < (int) parent.NValue)
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

        faceDownOffset *= n;
        faceDownOffset *= n;
    }

    private static void RemoveImplicitAnimations(Visual container)
    {
        if (ElementComposition.GetElementVisual(container) is { } compositionVisual)
        {
            compositionVisual.ImplicitAnimations = null;
        }
    }
}