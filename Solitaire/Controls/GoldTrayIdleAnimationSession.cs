using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Styling;
using Solitaire.Utils;

namespace Solitaire.Controls;

internal sealed class GoldTrayIdleAnimationSession
{
    internal static readonly TimeSpan RestDuration = TimeSpan.FromSeconds(57);
    private readonly Animatable _target;
    private readonly GoldTraySurface _surface;
    private CancellationTokenSource? _cancellation;

    public GoldTrayIdleAnimationSession(Visual target, GoldTraySurface surface)
    {
        _target = target;
        _surface = surface;
    }

    public void Attach() => Start();

    public void Detach() => Stop();

    private void Start()
    {
        if (_cancellation is not null)
            return;

        var cancellation = new CancellationTokenSource();
        _cancellation = cancellation;
        Run(cancellation);
    }

    private void Stop() => _cancellation?.Cancel();

    private async void Run(CancellationTokenSource cancellation)
    {
        try
        {
            while (true)
            {
                await RunBurst(cancellation.Token);
                await Task.Delay(RestDuration, cancellation.Token);
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
        finally
        {
            if (ReferenceEquals(_cancellation, cancellation))
                _cancellation = null;
            cancellation.Dispose();
        }
    }

    private Task RunBurst(CancellationToken token)
    {
        return _surface switch
        {
            GoldTraySurface.Outer => Task.WhenAll(
                RunResource("GoldenTrayOuterBorderIdleAnimation", token),
                RunResource("GoldenTrayOuterBackgroundIdleAnimation", token)),
            GoldTraySurface.Inner => RunResource("GoldenTrayInnerBorderIdleAnimation", token),
            _ => throw new InvalidOperationException($"Unknown gold tray surface: {_surface}.")
        };
    }

    private Task RunResource(string resourceKey, CancellationToken token)
    {
        var animation = (Animation)Application.Current!.FindResource(resourceKey)!;
        return CasinoAnimations.Run(animation, _target, token);
    }
}
