using System;
using System.Threading;
using System.Threading.Tasks;
using Solitaire.Utils;

namespace Solitaire.Controls;

internal sealed class GoldControlAnimation : IDisposable
{
    private readonly GoldButtonChrome _chrome;
    private CancellationTokenSource? _focus;
    private CancellationTokenSource? _press;
    internal bool HasRunningEffects => _focus is not null || _press is not null;
    internal bool IsResting { get; private set; }

    public GoldControlAnimation(GoldButtonChrome chrome)
    {
        _chrome = chrome;
    }

    public void SetActive(bool active)
    {
        if (!active)
        {
            _focus?.Cancel();
            _focus = null;
        }
        else if (_focus is null)
        {
            _focus = new CancellationTokenSource();
            RunFocus(_focus);
        }
    }

    public void Press()
    {
        _press?.Cancel();
        _press = new CancellationTokenSource();
        RunPress(_press);
    }

    public void Stop()
    {
        SetActive(false);
        _press?.Cancel();
        _press = null;
    }

    public void Dispose() => Stop();

    private async void RunFocus(CancellationTokenSource cancellation)
    {
        try
        {
            while (!cancellation.IsCancellationRequested)
            {
                IsResting = false;
                await CasinoAnimations.Run(CasinoAnimations.Create("GoldFocusAnimation"), _chrome, cancellation.Token);
                cancellation.Token.ThrowIfCancellationRequested();
                IsResting = true;
                await Task.Delay(GoldTrayIdleAnimationSession.RestDuration, cancellation.Token);
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
        finally
        {
            if (ReferenceEquals(_focus, cancellation))
                _focus = null;
            cancellation.Dispose();
        }
    }

    private async void RunPress(CancellationTokenSource cancellation)
    {
        try
        {
            await CasinoAnimations.Run(CasinoAnimations.Create("GoldPressAnimation"), _chrome, cancellation.Token);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
        finally
        {
            if (ReferenceEquals(_press, cancellation))
                _press = null;
            cancellation.Dispose();
        }
    }
}
