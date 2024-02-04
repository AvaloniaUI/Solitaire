using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Solitaire.Models;

namespace Solitaire.Controls;

public class SuitMarker : TemplatedControl
{
    public static readonly StyledProperty<CardSuit> SuitProperty =
        AvaloniaProperty.Register<TemplatedControl, CardSuit>(nameof(Suit));

    public CardSuit Suit
    {
        get => GetValue(SuitProperty);
        set => SetValue(SuitProperty, value);
    }
}