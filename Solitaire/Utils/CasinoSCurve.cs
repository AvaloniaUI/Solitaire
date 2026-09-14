using System;
using Avalonia.Animation.Easings;

namespace Solitaire.Utils;

// Symmetric acceleration and deceleration for the casino's light transitions.
public sealed class CasinoSCurve : Easing
{
    public override double Ease(double progress)
    {
        var t = Math.Clamp(progress, 0, 1);
        return t < .5 ? (Math.Pow(10, t * 2) - 1) / 18 : 1 - (Math.Pow(10, (1 - t) * 2) - 1) / 18;
    }
}
