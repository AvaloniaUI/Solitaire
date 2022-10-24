using System;
using Avalonia;
using SolitaireAvalonia.Utils;
using SolitaireAvalonia.ViewModels;

namespace SolitaireAvalonia.Desktop;

class Program
{

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        PlatformProviders.CasinoStorage = new DesktopSettingsStore<CasinoViewModel>();
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new X11PlatformOptions() { UseGpu = false })
            .LogToTrace();
}