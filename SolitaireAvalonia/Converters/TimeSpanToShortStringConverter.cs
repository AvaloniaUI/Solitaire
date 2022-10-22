using System;
using Avalonia.Data.Converters;

namespace SolitaireAvalonia.Converters
{
    /// <summary>
    /// A converter that turns a time span into a small string, only 
    /// suitable for up to 24 hours.
    /// </summary>
    class TimeSpanToShortStringConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TimeSpan timeSpan = (TimeSpan)value;
            if(timeSpan.Hours > 0)
                return string.Format("{0:D2}:{1:D2}:{2:D2}",
                        timeSpan.Hours,
                        timeSpan.Minutes,
                        timeSpan.Seconds);
            else
                return string.Format("{0:D2}:{1:D2}",
                        timeSpan.Minutes,
                        timeSpan.Seconds);
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
