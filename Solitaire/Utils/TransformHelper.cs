using Avalonia;
using Avalonia.Media.Immutable;

namespace Solitaire.Utils;

public class TransformHelper
{
    public static ImmutableTransform Rotate120 { get; } =
        new ImmutableTransform(Matrix.CreateRotation(120 * 0.0174533));
}