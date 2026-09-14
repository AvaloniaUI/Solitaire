using System;
using Avalonia;
using Avalonia.Controls;
using Solitaire.Controls;

namespace Solitaire.Views.Pages;

public sealed partial class GameStatisticsView : UserControl, IDisposable
{
    private StatisticsRowMotion? _rowMotion;

    public GameStatisticsView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Dispose();
        // Initialize every row before the first rendered frame of this visit.
        _rowMotion = new StatisticsRowMotion((Grid)Content!);
        _ = _rowMotion.Run();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Dispose();
        base.OnDetachedFromVisualTree(e);
    }

    public void Dispose()
    {
        _rowMotion?.Dispose();
        _rowMotion = null;
    }
}
