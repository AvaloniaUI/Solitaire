using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Solitaire;
using Solitaire.Browser;
using Solitaire.Models;
using Solitaire.Utils;

internal sealed partial class Program
{
    private static Task Main(string[] args)
    {
        PlatformProviders.CasinoStorage = new BrowserSettingsStore<PersistentState>();

        return BuildAvaloniaApp()
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}