﻿// using System;
// using System.Collections.Generic;
// using System.Globalization;
// using Avalonia;
// using Avalonia.Data.Converters;
// using Avalonia.Media;
// using Solitaire.Models;
//
// namespace Solitaire.Converters;
//
// /// <summary>
// /// Converter to get the brush for a playing card.
// /// </summary>
// public class PlayingCardToBrushConverter : IValueConverter
// {
//     /// <summary>
//     /// A dictionary of brushes for card types.
//     /// </summary>
//     private static readonly Dictionary<string, DrawingImage> Brushes = new(); 
//
//     /// <inheritdoc />
//     public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
//     {
//         if (value is not CardType cardType) return null;
//
//         var cardName = cardType.ToString();
//
//         if (Brushes.TryGetValue( cardName, out var retDrawingImage))
//         {
//             return retDrawingImage;
//         }
//  
//         if (!Application.Current!.Styles.TryGetResource(cardName, null, out var test) ||
//             test is not DrawingImage faceImage) return null;
//
//         Brushes.Add(cardName, faceImage);
//
//         return faceImage;
//     }
//
//     /// <inheritdoc />
//     public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
//     {
//         return null;
//     }
// }