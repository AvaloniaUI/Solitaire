using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using Microsoft.CodeAnalysis.Operations;
using Solitaire.Models;
using Solitaire.ViewModels;
using Solitaire.ViewModels.Pages;

namespace Solitaire.Behaviors;

public class CardFieldBehavior : Behavior<Canvas>
{
    private static ImplicitAnimationCollection? ImplicitAnimations;

    public static readonly AttachedProperty<ObservableCollection<StacksMetadata>> CardStacksProperty =
        AvaloniaProperty.RegisterAttached<CardFieldBehavior, Control, ObservableCollection<StacksMetadata>>(
            "CardStacks");

    public static void SetCardStacks(Control obj, ObservableCollection<StacksMetadata> value) =>
        obj.SetValue(CardStacksProperty, value);

    public static ObservableCollection<StacksMetadata>? GetCardStacks(Control obj) => obj.GetValue(CardStacksProperty);


    public static readonly AttachedProperty<List<PlayingCardViewModel>> CardsProperty =
        AvaloniaProperty.RegisterAttached<CardFieldBehavior, Control, List<PlayingCardViewModel>>("Cards");

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
        base.OnAttached();
    }

    private void AssociatedObjectOnLoaded(object? sender, RoutedEventArgs e)
    {
        if (AssociatedObject == null) return;

        EnsureImplicitAnimations();

        var cardsList = GetCards(AssociatedObject);
        var cardStacks = GetCardStacks(AssociatedObject);

        if (cardStacks != null)
            cardStacks.CollectionChanged += XssOnCollectionChanged;
        
        if (Application.Current == null ||
            !Application.Current.Styles.TryGetResource("PlayingCardDataTemplate", out var x) ||
            x is not DataTemplate y) return;

        AssociatedObject.DataTemplates.Add(y);

        var homePosition = cardStacks?.FirstOrDefault(i => i.IsHomeStack)?.StackOrigin ?? new Point(); 

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

            AddImplicitAnimations(container);

            cardsList.Add(card);

            AssociatedObject.Children.Add(container);
        }
    }
 
    private void XssOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var cardsList = GetCards(AssociatedObject);

        var cardStacks = GetCardStacks(AssociatedObject);

        if (cardStacks != null)
            cardStacks.CollectionChanged += XssOnCollectionChanged;
        
        foreach (var card in cardsList)
        {
            
            
        }
    }


    private static void AddImplicitAnimations(Visual container)
    {
        if (ElementComposition.GetElementVisual(container) is { } compositionVisual)
        {
            compositionVisual.ImplicitAnimations = ImplicitAnimations;
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