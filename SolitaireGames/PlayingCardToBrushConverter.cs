using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace SolitaireGames
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

        /// <summary>
        /// Converts source values to a value for the binding target. The data binding engine calls this method when it propagates the values from source bindings to the binding target.
        /// </summary>
        /// <param name="values">The array of values that the source bindings in the <see cref="T:System.Windows.Data.MultiBinding"/> produces. The value <see cref="F:System.Windows.DependencyProperty.UnsetValue"/> indicates that the source binding has no value to provide for conversion.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value.If the method returns null, the valid null value is used.A return value of <see cref="T:System.Windows.DependencyProperty"/>.<see cref="F:System.Windows.DependencyProperty.UnsetValue"/> indicates that the converter did not produce a value, and that the binding will use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue"/> if it is available, or else will use the default value.A return value of <see cref="T:System.Windows.Data.Binding"/>.<see cref="F:System.Windows.Data.Binding.DoNothing"/> indicates that the binding does not transfer the value or use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue"/> or the default value.
        /// </returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
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
                brushes.Add(imageSource, new ImageBrush(new BitmapImage(new Uri(imageSource))));

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
