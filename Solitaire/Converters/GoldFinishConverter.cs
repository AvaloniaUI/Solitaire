using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Solitaire.Converters;

public sealed class GoldFinishConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 6 || values[0] is not Rect bounds || values[1] is not CornerRadius radius ||
            values[2] is not double focus || values[3] is not double phase ||
            values[4] is not double press || values[5] is not double sweep)
            return new GoldFinishMetrics(0, 0, 0, 0, 0, 0, 1);
        return new GoldFinishMetrics(Math.Max(0, bounds.Width - 2), Math.Max(0, bounds.Height - 2),
            radius.TopLeft, focus, phase, press, sweep);
    }
}

// Geometry follows the control size; the template defines the appearance.
public sealed class GoldFinishMetrics(double width, double height, double radius,
    double focus, double phase, double press, double sweep)
{
    private double Pulse => Math.Sin(Math.PI * Math.Clamp(phase, 0, 1));
    public Rect Face => new(0, 0, width, height);
    public double Radius => radius;
    public CornerRadius InnerRadius => new(Math.Max(0, radius - 2));
    public double Width => width;
    public double Height => height;
    public double InnerWidth => Math.Max(0, width - 4);
    public double InnerHeight => Math.Max(0, height - 4);
    public double RimWidth => Math.Max(0, width - radius * 2);
    public double Focus => focus;
    public double GlowOpacity => .16 + Pulse * .26;
    public double FirstGlowWidth => width * .8;
    public double FirstGlowHeight => height * 2.2;
    public double FirstGlowX => -width * .4;
    public double FirstGlowY => -height * 1.1;
    public double SecondGlowWidth => width * .64;
    public double SecondGlowHeight => height * 1.7;
    public double SecondGlowX => width * .68;
    public double SecondGlowY => height * .15;
    public double EdgeOpacity => Pulse;
    public double EdgeWidth => Math.Min(80, width * .35);
    public double TopEdgeX => (width - EdgeWidth) * phase;
    public double BottomEdgeX => width - EdgeWidth - TopEdgeX;
    public double BottomEdgeY => Math.Max(0, height - 2);
    public double GlintX => -EdgeWidth + (width + EdgeWidth + height * .25) * phase;
    public double PressOpacity => press * .14;
    public double PressOffset => press * 1.2;
    public double SweepOpacity => sweep is > 0 and < 1 ? Math.Sin(sweep * Math.PI) * .32 : 0;
    public double SweepWidth => Math.Min(110, width * .45);
    public double SweepX => -SweepWidth + (width + SweepWidth) * sweep;
}

public sealed class LetteringSweepConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        new RelativePoint((value is double sweep ? sweep : 1) * 2 - (parameter as string == "Start" ? .9 : .1), 0, RelativeUnit.Relative);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
