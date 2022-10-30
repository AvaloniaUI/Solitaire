using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Solitaire.ViewModels;
using Solitaire.Views;

namespace Solitaire;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();

            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                desktop.MainWindow.DataContext = await CasinoViewModel.Load();
            });
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new CasinoView();
            
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                singleViewPlatform.MainView.DataContext = await CasinoViewModel.Load();
            });
        }

        base.OnFrameworkInitializationCompleted();
    }
}