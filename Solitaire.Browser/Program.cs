using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Browser;
using Solitaire.Utils;
using Solitaire.ViewModels;

[assembly:SupportedOSPlatform("browser")]

namespace Solitaire.Browser;

internal class Program
{
    // ReSharper disable once UnusedParameter.Local
    private static void Main(string[] args)
    {
        PlatformProviders.CasinoStorage = new BrowserSettingsStore<CasinoViewModel>();
        
        BuildAvaloniaApp()
            .SetupBrowserApp("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}