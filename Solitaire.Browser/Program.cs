using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Web;
using Solitaire.Browser;
using Solitaire.Utils;
using Solitaire.ViewModels;

[assembly:SupportedOSPlatform("browser")]

internal partial class Program
{
    private static void Main(string[] args)
    {
        PlatformProviders.CasinoStorage = new BrowserSettingsStore<CasinoViewModel>();
        
        BuildAvaloniaApp()
            .SetupBrowserApp("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<Solitaire.App>();
}
