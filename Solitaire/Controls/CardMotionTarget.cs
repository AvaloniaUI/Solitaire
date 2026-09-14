using Avalonia;

namespace Solitaire.Controls;

internal readonly record struct CardMotionTarget(PlayingCard Card, Vector Position, double Turn, bool Deal);
