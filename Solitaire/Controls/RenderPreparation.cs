using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Solitaire.ViewModels;
using Solitaire.Views.Pages;

namespace Solitaire.Controls;

// Prepare rendering behind the loading screen on the application compositor.
internal sealed class RenderPreparation : IDisposable
{
    // Short animations prepare the same rendering paths before the menu appears.
    internal static readonly TimeSpan SampleDuration = TimeSpan.FromMilliseconds(64);
    private readonly Panel _owner;
    private readonly PreparationLayer _staging = new() { IsHitTestVisible = false, ClipToBounds = true };
    private readonly CancellationTokenSource _cancellation = new();
    private readonly CancellationToken _token;
    private bool _disposed;

    public RenderPreparation(Panel owner, IBrush? felt)
    {
        _owner = owner;
        _token = _cancellation.Token;
        KeyboardNavigation.SetTabNavigation(_staging, KeyboardNavigationMode.None);
        _staging.Children.Add(new Panel
        {
            Background = felt,
            ZIndex = int.MaxValue,
            Children = { new Border { Background = owner.Background } }
        });
        owner.Children.Insert(0, _staging);
    }

    public Task Run(Compositor compositor, CasinoViewModel casino) =>
        Task.WhenAll(PrepareScene(compositor, casino, false), PrepareScene(compositor, casino, true));

    private async Task PrepareScene(Compositor compositor, CasinoViewModel casino, bool game)
    {
        using var host = new ConnectedNavigationHost
        {
            TransitionDuration = SampleDuration,
            Content = new TitleView { DataContext = casino.TitleInstance }
        };
        _staging.Children.Insert(0, host);
        try
        {
            await Layout(host, compositor);
            await PrepareGoldEffects(host, compositor);
            if (game)
                await PrepareGame(host, compositor, casino);
            else
                await Navigate(host, new SettingsView { DataContext = casino.SettingsInstance }, compositor);
            await Navigate(host, new TitleView { DataContext = casino.TitleInstance }, compositor);
        }
        finally
        {
            _staging.Children.Remove(host);
            host.Content = null;
        }
    }

    private async Task Layout(Control content, Compositor compositor)
    {
        _token.ThrowIfCancellationRequested();
        await Dispatcher.UIThread.InvokeAsync(content.UpdateLayout, DispatcherPriority.Loaded, _token);
        _token.ThrowIfCancellationRequested();
        await compositor.RequestCompositionBatchCommitAsync().Rendered.WaitAsync(_token);
        _token.ThrowIfCancellationRequested();
    }

    private async Task PrepareGoldEffects(Control page, Compositor compositor)
    {
        var chrome = page.GetVisualDescendants().OfType<GoldButtonChrome>().FirstOrDefault();
        if (chrome is null)
            return;
        // Prepare highlights and lettering behind the loading screen.
        using var focus = chrome.SetValue(GoldButtonChrome.FocusAmountProperty, 1d, BindingPriority.Animation);
        using var glint = chrome.SetValue(GoldButtonChrome.GlintProgressProperty, .4, BindingPriority.Animation);
        using var press = chrome.SetValue(GoldButtonChrome.PressSweepProperty, .35, BindingPriority.Animation);
        await Layout(page, compositor);
    }

    private async Task Navigate(ConnectedNavigationHost host, Control page, Compositor compositor)
    {
        _token.ThrowIfCancellationRequested();
        host.Content = page;
        await Layout(host, compositor);
        await host.TransitionTask.WaitAsync(_token);
        await Layout(host, compositor);
    }

    private async Task PrepareGame(ConnectedNavigationHost host, Compositor compositor, CasinoViewModel casino)
    {
        // A null context prevents changes to the player game.
        // Status controls use the model to keep the correct board scale.
        var board = new SpiderSolitaireView { DataContext = null };
        var dock = ((Panel)board.Content!).Children.OfType<DockPanel>().Single();
        var status = dock.Children.OfType<ContentControl>().Single();
        status.DataContext = casino.SpiderInstance;
        using var cards = new CardRenderPreparation(_token);
        board.FindControl<Canvas>("PlayingField")!.Children.Add(cards.Scene);
        try
        {
            host.Content = board;
            await Layout(host, compositor);
            await Task.WhenAll(host.TransitionTask, cards.Animate()).WaitAsync(_token);
            await Layout(host, compositor);
        }
        finally
        {
            status.DataContext = null;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _cancellation.Cancel();
        _cancellation.Dispose();
        _staging.Children.Clear();
        _owner.Children.Remove(_staging);
    }
}
