using System;
using Avalonia;
using Avalonia.Media;

namespace Solitaire.Models;

public static class TableLighting
{
    public const double DefaultDirection = 315;
    public const double DefaultElevation = 55;
    public static double Direction { get; private set; } = DefaultDirection;
    public static double Elevation { get; private set; } = DefaultElevation;
    public static event Action? Changed;
    internal static int SubscriberCount => Changed?.GetInvocationList().Length ?? 0;

    public static void Set(double direction, double elevation)
    {
        direction = double.IsFinite(direction) ? Math.Clamp(direction, 0, 360) : DefaultDirection;
        elevation = double.IsFinite(elevation) ? Math.Clamp(elevation, 20, 80) : DefaultElevation;
        if (Direction == direction && Elevation == elevation)
            return;
        Direction = direction;
        Elevation = elevation;
        Changed?.Invoke();
    }

    // Direction names the light's location: north is 0 degrees, east is 90.
    public static Vector ShadowOffset(double height, double direction, double elevation)
    {
        var angle = direction * Math.PI / 180;
        var distance = height / Math.Tan(elevation * Math.PI / 180);
        return new Vector(-Math.Sin(angle) * distance, Math.Cos(angle) * distance);
    }

    public static BoxShadows ContactShadow()
    {
        var offset = ShadowOffset(1.2, Direction, Elevation);
        return new BoxShadows(new BoxShadow
        {
            OffsetX = offset.X,
            OffsetY = offset.Y,
            Blur = 2,
            Color = Color.FromArgb(75, 0, 0, 0)
        });
    }
}
