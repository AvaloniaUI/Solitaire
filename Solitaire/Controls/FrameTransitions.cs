using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using Solitaire.Utils;

namespace Solitaire.Controls;

internal static class FrameTransitions
{
    public static void Place(AnimatedTrayFrame frame, ConnectedFrameState state)
    {
        frame.FramePosition = state.Bounds.Position;
        frame.FrameSize = state.Bounds.Size;
        frame.FrameScale = state.Scale;
        frame.FrameShadow = state.Shadow;
    }

    public static async Task Run(AnimatedTrayFrame frame, ConnectedFrameState target, CancellationToken token,
        TimeSpan? animationDuration = null)
    {
        if (token.IsCancellationRequested)
            return;
        var transitions = CasinoAnimations.CreateTransitions("ConnectedFrameTransitions", animationDuration);
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        void Changed(object? sender, AvaloniaPropertyChangedEventArgs change)
        {
            if (frame.State == target)
                completion.TrySetResult();
        }
        void Release()
        {
            if (!ReferenceEquals(frame.Transitions, transitions))
                return;
            // Make the displayed values the base before releasing animation priority.
            var displayed = frame.State;
            Place(frame, displayed);
            frame.Transitions = null;
        }
        frame.PropertyChanged += Changed;
        frame.Transitions = transitions;
        using var cancellation = token.Register(() =>
        {
            Release();
            completion.TrySetResult();
        });
        try
        {
            Place(frame, target);
            if (frame.State == target)
                completion.TrySetResult();
            await completion.Task;
            await Dispatcher.Yield(DispatcherPriority.Normal);
        }
        finally
        {
            frame.PropertyChanged -= Changed;
            Release();
        }
    }
}
