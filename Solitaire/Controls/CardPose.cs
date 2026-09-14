using System;
using Avalonia;
using Solitaire.Models;

namespace Solitaire.Controls;

// Orthographic card pose. Turn is zero for the back and one for the face.
internal readonly record struct CardPose(Vector Position, double Turn, double Lift)
{
    public double WidthScale => Math.Abs(Math.Cos(Turn * Math.PI));
    private double Rise(double width) => Turn is 0 or 1 ? 0 : Math.Sin(Turn * Math.PI) * width / 2;
    public double CenterHeight(double width) => Lift + Math.Abs(Rise(width));

    public Matrix Artwork(Size size) => new(WidthScale, 0, 0, 1,
        Position.X + size.Width * (1 - WidthScale) / 2, Position.Y);

    public CardShadowShape Project(Size size)
    {
        var cosine = Math.Cos(Turn * Math.PI);
        var rise = Rise(size.Width);
        var center = (Point)Position + new Vector(size.Width / 2, 0);
        var height = CenterHeight(size.Width) + 1.2;
        var left = center + new Vector(-size.Width * cosine / 2, 0) + Offset(height - rise);
        var right = center + new Vector(size.Width * cosine / 2, 0) + Offset(height + rise);
        if (right.X < left.X)
            (left, right) = (right, left);
        return new CardShadowShape(left, right, right + new Vector(0, size.Height),
            left + new Vector(0, size.Height), size);
    }

    private static Vector Offset(double height) =>
        TableLighting.ShadowOffset(height, TableLighting.Direction, TableLighting.Elevation);
}
