using System;
using System.Threading;
using System.Threading.Tasks;

namespace Solitaire.ViewModels;

public abstract partial class CardGameViewModel : IDisposable
{
    private CancellationTokenSource _actionCancellation = new();
    private int _viewGeneration;
    private bool _disposed;

    protected internal CancellationToken ActionCancellation => _actionCancellation.Token;

    protected internal static async Task<bool> PauseGameAction(int milliseconds, CancellationToken cancellation)
    {
        try
        {
            await Task.Delay(milliseconds, cancellation);
            return !cancellation.IsCancellationRequested;
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            return false;
        }
    }

    private void ResetPendingActions()
    {
        var previous = _actionCancellation;
        _actionCancellation = new CancellationTokenSource();
        previous.Cancel();
        previous.Dispose();
        if (Deck is { } deck)
            foreach (var card in deck)
                card.Reset();
    }

    internal void ResetForNavigation()
    {
        _viewGeneration++;
        ResetGame();
    }

    internal int BeginView()
    {
        ResetForNavigation();
        return _viewGeneration;
    }

    internal void EndView(int generation)
    {
        if (generation == _viewGeneration)
            ResetForNavigation();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed || !disposing)
            return;
        _disposed = true;
        _viewGeneration++;
        _actionCancellation.Cancel();
        _actionCancellation.Dispose();
        StopTimer();
        _timer.Tick -= timer_Tick;
    }
}
