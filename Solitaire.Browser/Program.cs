using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Avalonia;
using Avalonia.Browser;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Rendering;
using Avalonia.Threading;
using Solitaire;
using Solitaire.Browser;
using Solitaire.Models;
using Solitaire.Utils;

internal sealed partial class Program
{
    private static Task Main(string[] args)
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
        PlatformProviders.CasinoStorage = new BrowserSettingsStore<PersistentState>();

        var (options, overlays) = ParseArgs(args);

        return BuildAvaloniaApp()
            .AfterSetup(_ =>
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (Application.Current!.ApplicationLifetime is ISingleViewApplicationLifetime lifetime
                        && overlays != default)
                    {
                        TopLevel.GetTopLevel(lifetime.MainView)!.RendererDiagnostics.DebugOverlays = overlays;
                    }
                }, DispatcherPriority.Background);
            })
            .StartBrowserAppAsync("out", options ?? new BrowserPlatformOptions());
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();

    private static (BrowserPlatformOptions? options, RendererDebugOverlays overlays) ParseArgs(string[] args)
    {
        try
        {
            if (args.Length == 0
                || !Uri.TryCreate(args[0], UriKind.Absolute, out var uri)
                || uri.Query.Length <= 1)
            {
                uri = new Uri("http://localhost");
            }

            var queryParams = HttpUtility.ParseQueryString(uri.Query);
            var options = new BrowserPlatformOptions();

            if (bool.TryParse(queryParams[nameof(options.PreferFileDialogPolyfill)], out var preferDialogsPolyfill))
            {
                options.PreferFileDialogPolyfill = preferDialogsPolyfill;
            }

            if (bool.TryParse(queryParams[nameof(options.PreferManagedThreadDispatcher)], out var preferManagedThreadDispatcher))
            {
                options.PreferManagedThreadDispatcher = preferManagedThreadDispatcher;
            }

            if (queryParams[nameof(options.RenderingMode)] is { } renderingModePairs)
            {
                options.RenderingMode = renderingModePairs
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(entry => Enum.Parse<BrowserRenderingMode>(entry, true))
                    .ToArray();
            }

            Enum.TryParse<RendererDebugOverlays>(queryParams[nameof(RendererDiagnostics.DebugOverlays)], out var debugOverlay);

            Console.WriteLine("DebugOverlays: " + debugOverlay);
            Console.WriteLine("PreferFileDialogPolyfill: " + options.PreferFileDialogPolyfill);
            Console.WriteLine("PreferManagedThreadDispatcher: " + options.PreferManagedThreadDispatcher);
            Console.WriteLine("RenderingMode: " + string.Join(";", options.RenderingMode));

            return (options, debugOverlay);
        }
        catch (Exception ex)
        {
            Console.WriteLine("ParseArgs of BrowserPlatformOptions failed: " + ex);
            return default;
        }
    }
}