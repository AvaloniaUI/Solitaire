using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Solitaire.Models;

namespace Solitaire.Converters;

public sealed class LightDialConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) =>
        values.Count == 3 && values[0] is Rect bounds && values[1] is double direction && values[2] is double elevation
            ? new LightDialMetrics(bounds.Size, direction, elevation)
            : new LightDialMetrics(default, TableLighting.DefaultDirection, TableLighting.DefaultElevation);
}

public sealed class LightDialMetrics
{
    public LightDialMetrics(Size size, double direction, double elevation)
    {
        var center = new Point(size.Width / 2, size.Height / 2);
        var radius = Math.Max(0, Math.Min(size.Width, size.Height) / 2 - 13);
        IsVisible = radius > 15;
        OuterDiameter = (radius + 3) * 2;
        InnerDiameter = Math.Max(0, (radius - 1) * 2);
        TableDiameter = Math.Max(0, (radius - 19) * 2);
        FocusDiameter = (radius + 7) * 2;
        var light = At(center, radius - 1, direction * Math.PI / 180);
        LightX = light.X - 11;
        LightY = light.Y - 11;
        ShadowOffset = TableLighting.ShadowOffset(9, direction, elevation);
        Ticks = new LightDialTick[32];
        for (var tick = 0; tick < Ticks.Length; tick++)
        {
            var angle = tick * Math.PI / 16;
            var major = tick % 4 == 0;
            Ticks[tick] = new LightDialTick(At(center, radius - (major ? 13 : 10), angle),
                At(center, radius - 6, angle), major);
        }
    }

    public bool IsVisible { get; }
    public double OuterDiameter { get; }
    public double InnerDiameter { get; }
    public double TableDiameter { get; }
    public double FocusDiameter { get; }
    public double LightX { get; }
    public double LightY { get; }
    public Vector ShadowOffset { get; }
    public LightDialTick[] Ticks { get; }

    private static Point At(Point center, double radius, double angle) =>
        center + new Vector(Math.Sin(angle) * radius, -Math.Cos(angle) * radius);
}

public sealed record LightDialTick(Point Start, Point End, bool Major);
