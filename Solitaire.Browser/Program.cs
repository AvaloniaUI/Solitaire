using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Solitaire.Models;
using Solitaire.Utils;
using Solitaire.ViewModels;

[assembly:SupportedOSPlatform("browser")]

namespace Solitaire.Browser;

internal class Program
{
    // ReSharper disable once UnusedParameter.Local
    private static async Task Main(string[] args)
    {
        PlatformProviders.CasinoStorage = new BrowserSettingsStore<PersistentState>();

       await BuildAvaloniaApp()
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}