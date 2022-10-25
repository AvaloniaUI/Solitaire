using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Solitaire.Models;

namespace Solitaire.Converters;

/// <summary>
/// Converter to get the brush for a playing card.
/// </summary>
public class PlayingCardToBrushConverter : IMultiValueConverter
{
    /// <summary>
    /// Sets the deck folder.
    /// </summary>
    /// <param name="folderName">Name of the folder.</param>
    public static void SetDeckFolder(string folderName)
    {
        //  Clear the dictionary so we recreate card brushes.
        brushes.Clear();

        //  Set the deck folder.
        deckFolder = folderName;
    }

    /// <summary>
    /// The default deck folder.
    /// </summary>
    static string deckFolder = "Classic";

    /// <summary>
    /// A dictionary of brushes for card types.
    /// </summary>
    static Dictionary<string, object> brushes = new();

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        //  Cast the data.
        if (values[0] is not CardType cardType || values[1] is not bool isFaceDown) return null;
 

        if (isFaceDown && Application.Current!.Styles.TryGetResource($"CardBack", out var test1)
                       && test1 is DrawingImage c1)
        {
            return c1;
        }

        if (Application.Current!.Styles.TryGetResource($"{cardType.ToString()}", out var test)
            && test is DrawingImage c)
        {
            return c;
        }

        return null;
    }
}