using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Solitaire.Utils;

namespace Solitaire.Controls;

internal sealed partial class StatisticsRowMotion : Animatable, IDisposable
{
    [GeneratedStyledProperty]
    public partial double Progress { get; set; }
    private readonly MenuMotionPart[][] _rows;
    private readonly CancellationTokenSource _cancellation = new();
    private static readonly CasinoSCurve Ease = new();
    private bool _disposed;

    public StatisticsRowMotion(Grid grid)
    {
        // Pair each label and value, and skip hidden streak rows in Overall.
        _rows = grid.Children.Where(child => child.IsVisible).GroupBy(Grid.GetRow)
            .OrderBy(group => group.Key)
            .Select(group => group.Select(child => new MenuMotionPart(child, grid, true)).ToArray()).ToArray();
    }

    public async Task Run()
    {
        try
        {
            await CasinoAnimations.Run(CasinoAnimations.Create("StatisticsRowsAnimation"), this, _cancellation.Token);
        }
        catch (OperationCanceledException) when (_disposed) { }
        finally { Dispose(); }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property != ProgressProperty || _disposed)
            return;
        for (var index = 0; index < _rows.Length; index++)
        {
            var progress = Ease.Ease(Math.Clamp((Progress - .15 - index * .04) / .45, 0, 1));
            foreach (var part in _rows[index])
                part.Arrive(progress);
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _cancellation.Cancel();
        _cancellation.Dispose();
        foreach (var row in _rows)
            foreach (var part in row)
                part.Dispose();
    }
}
