using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Logging;
using Avalonia.Media;
using Solitaire.Models;

namespace Solitaire.Converters;

/// <summary>
/// Converter to get the brush for a playing card.
/// </summary>
public class PlayingCardToBrushConverter : IValueConverter
{
    // /// <summary>
    // /// A dictionary of brushes for card types.
    // /// </summary>
    private static readonly Dictionary<string, DrawingImage> Brushes = new();
    //
    // public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    // {
    //     //  Cast the data.
    //     if (values[0] is not CardType cardType || values[1] is not bool isFaceDown) return null;
    //
    //     var cardName = cardType.ToString();
    //     
    //     if (Brushes.TryGetValue(isFaceDown ? "CardBack" : cardName, out var retDrawingImage))
    //     {
    //         return retDrawingImage;
    //     }
    //     
    //     if (isFaceDown && Application.Current!.Styles.TryGetResource("CardBack", out var test1)
    //                    && test1 is DrawingImage backImage)
    //     {
    //         Brushes.Add("CardBack", backImage);
    //         return backImage;
    //     }
    //
    //     if (!Application.Current!.Styles.TryGetResource(cardName, out var test) ||
    //         test is not DrawingImage faceImage) return null;
    //     
    //     Brushes.Add(cardName, faceImage);
    //     return faceImage;
    // }


    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var logger = Logger.TryGet(LogEventLevel.Error, "SOL");

        if (value is not CardType cardType)
        {
            logger?.Log(null, $"{value} is not CardType");
            return null;
        }

        return Convert(cardType);
    }
    
    public static DrawingImage Convert(CardType cardType)
    {
        var logger = Logger.TryGet(LogEventLevel.Error, "SOL");
        var cardName = cardType.ToString();
        logger?.Log(null, $"Card name {cardName}");

        if (Brushes.TryGetValue( cardName, out var retDrawingImage))
        {
            return retDrawingImage;
        }

        if (!Application.Current!.Styles.TryGetResource(cardName, null, out var test) ||
            test is not DrawingImage faceImage)
        {
            logger?.Log(null, $"Unable to find resource for {cardName}");
            return null;
        }

        Brushes.Add(cardName, faceImage);
        
        return faceImage;
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}