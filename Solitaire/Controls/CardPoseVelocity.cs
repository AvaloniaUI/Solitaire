using Avalonia;

namespace Solitaire.Controls;

// Table units, turns, and elevation units per second.
internal readonly record struct CardPoseVelocity(Vector Position, double Turn, double Lift);
