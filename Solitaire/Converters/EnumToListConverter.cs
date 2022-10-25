using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace Solitaire.Converters;

public class EnumToListConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var retList = new List<object>();
        if (parameter is not Type { IsEnum: true } typeObj) return retList;
        retList.AddRange(typeObj.GetEnumValues().Cast<object>());
        return retList;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}