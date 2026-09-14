using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Logging;
using Avalonia.Rendering.Composition;
using Solitaire.Controls;
using Solitaire.ViewModels;

namespace Solitaire.Views;

public partial class CasinoView : UserControl
{
    private RenderPreparation? _preparation;
    private bool _preparationStarted;
    private readonly TaskCompletionSource _renderingReady = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _menuReady = new(TaskCreationOptions.RunContinuationsAsynchronously);
    internal Task? RenderPreparationTask { get; private set; }
    public Task RenderingReady => _renderingReady.Task;
    public Task MenuReady => _menuReady.Task;

    public CasinoView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _ = PrepareRendering();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        _ = PrepareRendering();
    }

    private async Task PrepareRendering()
    {
        if (!IsLoaded || _preparationStarted || Design.IsDesignMode || DataContext is not CasinoViewModel casino ||
            casino.CurrentView != casino.TitleInstance ||
            ElementComposition.GetElementVisual(this)?.Compositor is not { } compositor)
            return;
        _preparationStarted = true;
        using var preparation = new RenderPreparation(SceneRoot, Background);
        _preparation = preparation;
        void Navigating(object? sender, PropertyChangedEventArgs change)
        {
            if (change.PropertyName == nameof(CasinoViewModel.CurrentView))
                preparation.Dispose();
        }
        casino.PropertyChanged += Navigating;
        try
        {
            RenderPreparationTask = preparation.Run(compositor, casino);
            await compositor.RequestCompositionBatchCommitAsync().Rendered;
            _menuReady.TrySetResult();
            await RenderPreparationTask;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception error)
        {
            Logger.TryGet(LogEventLevel.Warning, "Rendering")?.Log(this, "Render preparation failed: {Error}", error);
        }
        finally
        {
            casino.PropertyChanged -= Navigating;
            preparation.Dispose();
            _preparation = null;
            // The HTML loader stays in place until the prepared menu reaches the window.
            await compositor.RequestCompositionBatchCommitAsync().Rendered;
            _menuReady.TrySetResult();
            _renderingReady.TrySetResult();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _preparation?.Dispose();
        base.OnDetachedFromVisualTree(e);
    }
}
