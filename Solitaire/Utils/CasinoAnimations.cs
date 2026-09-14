using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Styling;
using Avalonia.Threading;
using Solitaire.Controls;

namespace Solitaire.Utils;

internal static class CasinoAnimations
{
    public static Animation Create(string key, TimeSpan? duration = null, int delay = 0)
    {
        // A fresh resource keeps per-run timing separate from other animations.
        var animation = (Animation)new Styles.Animations()[key]!;
        if (duration is { } value)
            animation.Duration = value;
        animation.Delay = TimeSpan.FromMilliseconds(delay);
        return animation;
    }

    public static Transitions CreateTransitions(string key, TimeSpan? duration = null)
    {
        var transitions = (Transitions)new Styles.Animations()[key]!;
        if (duration is { } value)
            foreach (TransitionBase transition in transitions)
                transition.Duration = value;
        return transitions;
    }

    public static async Task Run(Animation animation, Animatable target, CancellationToken token)
    {
        await animation.RunAsync(target, token);
        // RunAsync returns before Avalonia applies the final pose.
        // Wait for cleanup before the next animation reads that pose.
        await Dispatcher.Yield(DispatcherPriority.Normal);
    }

    public static Task Connect(ConnectedFrameState source, ConnectedFrameState target,
        AnimatedTrayFrame frame, CancellationToken token, TimeSpan? duration = null)
    {
        FrameTransitions.Place(frame, source);
        return FrameTransitions.Run(frame, target, token, duration);
    }

    public static Animation FadeOut(double from, TimeSpan? duration)
    {
        var animation = Create("PageExitAnimation", duration);
        // An interrupted page can already be partly transparent.
        ((Setter)animation.Children[0].Setters[0]).Value = from;
        return animation;
    }
}
