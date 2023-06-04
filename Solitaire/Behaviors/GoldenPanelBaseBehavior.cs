using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

namespace Solitaire.Behaviors;

public abstract class GoldenPanelBaseBehavior : Behavior<Control>
{
    protected static Action<Rect?, bool>? UpdateTrayBoundsAction { get; set; }
    protected static Canvas? MasterCanvas { get; set; }
}