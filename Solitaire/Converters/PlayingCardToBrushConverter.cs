using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Solitaire.Models;

namespace Solitaire.Converters;

public sealed class PlayingCardToBrushConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2 || values[0] is not CardType cardType || values[1] is not bool faceDown)
            return null;

        // Prepare a face when the model turns it, before the visible turnover midpoint.
        // Retain an already loaded image when it turns back so its bitmap cache remains valid.
        if (faceDown)
            return BindingOperations.DoNothing;

        return Application.Current!.Styles.TryGetResource(cardType.ToString(), null, out var image)
            ? image as DrawingImage
            : null;
    }
}
