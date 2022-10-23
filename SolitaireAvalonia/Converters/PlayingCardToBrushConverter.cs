using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SolitaireAvalonia.Models;

namespace SolitaireAvalonia.Converters;

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
    static Dictionary<string, Brush> brushes = new();
 
    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        //  Cast the data.
        if (values[0] is CardType cardType && values[1] is bool isFaceDown)
        {
                
            //  We're going to create an image source.
            var imageSource = string.Empty;

            //  If the card is face down, we're using the 'Rear' image.
            //  Otherwise it's just the enum value (e.g. C3, SA).
            if (isFaceDown)
                imageSource = "Back";
            else
                imageSource = cardType.ToString();

            var assetLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();

            //  Turn this string into a proper path.
            imageSource = $"avares://{nameof(SolitaireAvalonia)}/Assets/Decks/{deckFolder}/{imageSource}.png";

            //  Do we need to add this brush to the static dictionary?
            if (brushes.ContainsKey(imageSource) == false)
            {
                var image = assetLoader.Open(new Uri(imageSource));

                var k = new ImageBrush(new Bitmap(image));
                brushes.Add(imageSource, k);
            }

            //  Return the brush.
            return brushes[imageSource];
        }

        return null;
    }
}