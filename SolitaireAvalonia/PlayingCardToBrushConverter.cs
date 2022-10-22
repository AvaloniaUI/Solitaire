using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Avalonia.Data;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
namespace SolitaireAvalonia
{
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
        static Dictionary<string, Brush> brushes = new Dictionary<string, Brush>();


        /// <inheritdoc />
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            //  Cast the data.
            if (values == null || values.Count() != 2)
                return null;

            //  Cast the values.
            CardType cardType = (CardType)values[0];
            bool faceDown = (bool)values[1];

            //  We're going to create an image source.
            string imageSource = string.Empty;

            //  If the card is face down, we're using the 'Rear' image.
            //  Otherwise it's just the enum value (e.g. C3, SA).
            if (faceDown)
                imageSource = "Back";
            else
                imageSource = cardType.ToString();

            //  Turn this string into a proper path.
            imageSource = "pack://application:,,,/SolitaireGames;component/Resources/Decks/" + deckFolder + "/" + imageSource + ".png";

            //  Do we need to add this brush to the static dictionary?
            if (brushes.ContainsKey(imageSource) == false)
            {
                Debug.WriteLine("TODO: Images from playing card");
            }
                // brushes.Add(imageSource, new ImageBrush(new Image()));

            //  Return the brush.
            return brushes[imageSource];
        }

        /// <summary>
        /// Converts a binding target value to the source binding values.
        /// </summary>
        /// <param name="value">The value that the binding target produces.</param>
        /// <param name="targetTypes">The array of types to convert to. The array length indicates the number and types of values that are suggested for the method to return.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// An array of values that have been converted from the target value back to the source values.
        /// </returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
 
    }
}
