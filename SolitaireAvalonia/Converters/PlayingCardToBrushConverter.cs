using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SolitaireAvalonia.Models;
using SolitaireAvalonia.ViewModels;

namespace SolitaireAvalonia.Converters
{
    /// <summary>
    /// Converter to get the brush for a playing card.
    /// </summary>
    public class PlayingCardToBrushConverter : IValueConverter
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
        static Dictionary<string, Brush> brushes = new Dictionary<string, Brush>();

        /// <inheritdoc />
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            //  Cast the data.
            if (value is not null && value is PlayingCard pc)
            {
                //  We're going to create an image source.
                string imageSource = string.Empty;

                //  If the card is face down, we're using the 'Rear' image.
                //  Otherwise it's just the enum value (e.g. C3, SA).
                if (pc.IsFaceDown)
                    imageSource = "Back";
                else
                    imageSource = pc.CardType.ToString();

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

        /// <inheritdoc />
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}