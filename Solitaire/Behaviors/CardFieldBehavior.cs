using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using Solitaire.Controls;
using Solitaire.Models;
using Solitaire.ViewModels;
using Solitaire.Views.Pages;

namespace Solitaire.Behaviors;

public class CardFieldBehavior : Behavior<Canvas>
{
    private static ImplicitAnimationCollection? ImplicitAnimations;


    public static readonly AttachedProperty<List<CardStackControl>> CardStacksProperty =
        AvaloniaProperty.RegisterAttached<CardFieldBehavior, Control, List<CardStackControl>>("CardStacks");

    public static void SetCardStacks(Control obj, List<CardStackControl> value) => obj.SetValue(CardStacksProperty, value);
    public static List<CardStackControl> GetCardStacks(Control obj) => obj.GetValue(CardStacksProperty);
    
    
    public static readonly AttachedProperty<List<PlayingCardViewModel>> CardsProperty =
        AvaloniaProperty.RegisterAttached<CardFieldBehavior, Control, List<PlayingCardViewModel>>("Cards");

    public static void SetCards(Control obj, List<PlayingCardViewModel> value) => obj.SetValue(CardsProperty, value);
    public static List<PlayingCardViewModel> GetCards(Control obj) => obj.GetValue(CardsProperty);

    private void EnsureImplicitAnimations()
    {
        if (ImplicitAnimations != null)
        {
            return;
        }

        if (AssociatedObject == null) return;
        var x = AssociatedObject.GetVisualRoot() as Visual;
        var u = ElementComposition.GetElementVisual(x);
        var compositor =u.Compositor;

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
    /// <inheritdoc />
    protected override void OnAttachedToVisualTree()
    {
        base.OnAttachedToVisualTree();
        if (AssociatedObject == null) return;
        
        EnsureImplicitAnimations();
            
         
        var cardsList = GetCards(AssociatedObject);

        if (cardsList.Count != 0)
        {
            return;
            ;
        }
            
        var stacksList = new List<CardStackControl>();
        SetCardStacks(AssociatedObject, stacksList);

        if (Application.Current == null ||
            !Application.Current.Styles.TryGetResource("PlayingCardDataTemplate", out var x) ||
            x is not DataTemplate y) return;
        
        AssociatedObject.DataTemplates.Add(y);

        foreach (var cardType in Enum.GetValuesAsUnderlyingType(typeof(CardType)).Cast<CardType>())
        { 
            var card = new PlayingCardViewModel(AssociatedObject.DataContext as CardGameViewModel)
            {
                CardType = cardType, 
                IsFaceDown = true
            };
                    
            var container = new ContentControl()
            {
                Content = card
            };
                    
            AddImplicitAnimations(container);

            cardsList.Add(card);

            AssociatedObject.Children.Add(container);
                    
        }

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

}