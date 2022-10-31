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
using Avalonia.Threading;
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
        AssociatedObject.AttachedToVisualTree += AssociatedObjectOnAttachedToVisualTree;
        AssociatedObject.DetachedFromVisualTree += AssociatedObjectOnDetachedFromVisualTree;
        base.OnAttached();
    }

    private void AssociatedObjectOnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
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

    private void AssociatedObjectOnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (AssociatedObject == null) return;

    //    EnsureImplicitAnimations();

        var cardsList = GetCards(AssociatedObject);
        var cardStacks = GetCardStacks(AssociatedObject);

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
                Content = card,
                ZIndex = -1
            };
            
            container.Classes.Add("playingCard");
            _containerCache.Add(cardType, container);
            AssociatedObject.Children.Add(container);
            
            Canvas.SetLeft(container, homePosition.X);
            Canvas.SetTop(container, homePosition.Y);
        //   AddImplicitAnimations(container);
 
            cardsList.Add(card);
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
            !_containerCache.TryGetValue(newItem.CardType, out var container)) return;

        if (control.SourceItems == null) return;

        var index = e.NewStartingIndex;

        var sumOffsets = control.SourceItems.Select((card, i) => (card, i)).Where(tuple => tuple.i < index)
            .Select(z =>
            {
                GetOffsets(control, z.card, z.i, control.SourceItems.Count, out var xx,
                    out var yy);

                return z.card.IsFaceDown ? xx : yy;
            }).Sum();

        var pos = new Point(control.Bounds.Position.X +
                            (control.Orientation == Orientation.Horizontal ? sumOffsets : 0)
            , control.Bounds.Position.Y
              + (control.Orientation == Orientation.Vertical ? sumOffsets : 0)
        );

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
    }

    private static void RemoveImplicitAnimations(Visual container)
    {
        if (ElementComposition.GetElementVisual(container) is { } compositionVisual)
        {
            compositionVisual.ImplicitAnimations = null;
        }
    }
}