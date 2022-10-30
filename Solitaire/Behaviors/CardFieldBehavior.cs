using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Xaml.Interactivity;
using Solitaire.Models;
using Solitaire.ViewModels;
using Solitaire.Views.Pages;

namespace Solitaire.Behaviors;

public class CardFieldBehavior : Behavior<Canvas>
{
    public static readonly AttachedProperty<List<PlayingCardViewModel>> CardsProperty =
        AvaloniaProperty.RegisterAttached<CardFieldBehavior, Control, List<PlayingCardViewModel>>("Cards");

    public static void SetCards(Control obj, List<PlayingCardViewModel> value) => obj.SetValue(CardsProperty, value);
    public static List<PlayingCardViewModel> GetCards(Control obj) => obj.GetValue(CardsProperty);

    /// <inheritdoc />
    protected override void OnAttached()
    {
        var list = new List<PlayingCardViewModel>();
        if (AssociatedObject != null)
        {
            SetCards(AssociatedObject, list);

            if (Application.Current != null &&
                Application.Current.Styles.TryGetResource("PlayingCardDataTemplate", out var x) && x is DataTemplate y)
            {
                AssociatedObject.DataTemplates.Add(y);

                foreach (var card in Enum.GetValuesAsUnderlyingType(typeof(CardType)))
                {
                    var cardType = (CardType) card;

                    var g = new PlayingCardViewModel(AssociatedObject.DataContext as CardGameViewModel)
                    {
                        CardType = cardType
                    };

                    list.Add(g);

                    AssociatedObject.Children.Add(new ContentControl()
                    {
                        Content = g
                    });
                }
            }
        }

        base.OnAttached();
    }
}