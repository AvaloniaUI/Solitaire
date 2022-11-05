using System;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.Xaml.Interactions.Custom;

namespace Solitaire.Behaviors;

public abstract class CompositionBehaviors<T> : DisposingBehavior<T> where T : class, IAvaloniaObject
{
    public TimeSpan Duration { get; set; }
    public TimeSpan Delay { get; set; }
    public Easing? Easing { get; set; }
}