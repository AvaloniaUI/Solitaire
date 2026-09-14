using System.Runtime.Versioning;
using System.Linq;
using Avalonia;
using Avalonia.Browser;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.Threading.Tasks;
using Solitaire.Platform.Browser;
using Solitaire.Utils;
using Solitaire.ViewModels;
using Solitaire.Views;
using Solitaire.Controls;

[assembly: SupportedOSPlatform("browser")]

namespace Solitaire.Browser;

internal sealed class Program
{
    // ReSharper disable once UnusedParameter.Local
    private static async Task Main(string[] args)
    {
        PlatformProviders.CasinoStorage = new BrowserSettingsStore<CasinoViewModel>();

        await BuildAvaloniaApp()
            .StartBrowserAppAsync("out");

        var startup = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime
                { MainView: CasinoView view })
            {
                await view.MenuReady;
                // Use the visible menu, not the hidden preparation views.
                var menu = view.FindControl<Panel>("SceneRoot")!.Children.OfType<ConnectedNavigationHost>().Single();
                var artwork = menu.GetVisualDescendants().OfType<Viewbox>()
                    .FirstOrDefault(control => control.Name == "TitleArtwork");
                if (artwork?.TransformToVisual(view) is { } transform && view.Bounds.Width > 0 && view.Bounds.Height > 0)
                {
                    var bounds = new Rect(artwork.Bounds.Size).TransformToAABB(transform);
                    // HTML and Avalonia can use different coordinate scales.
                    return (Bounds: new Rect(bounds.X / view.Bounds.Width, bounds.Y / view.Bounds.Height,
                        bounds.Width / view.Bounds.Width, bounds.Height / view.Bounds.Height), Ready: view.RenderingReady);
                }
                return (Bounds: default(Rect), Ready: view.RenderingReady);
            }
            return (Bounds: default(Rect), Ready: Task.CompletedTask);
        });
        LoaderInterop.Begin(startup.Bounds.X, startup.Bounds.Y, startup.Bounds.Width, startup.Bounds.Height);
        await startup.Ready;
        LoaderInterop.Ready();
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}
