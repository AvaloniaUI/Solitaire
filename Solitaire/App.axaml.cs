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

    public override async void OnFrameworkInitializationCompleted()
    {
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.MainWindow = new MainWindow();
                desktop.MainWindow.DataContext = await CasinoViewModel.Load();
                break;
            case ISingleViewApplicationLifetime singleViewPlatform:
                singleViewPlatform.MainView = new CasinoView();
                singleViewPlatform.MainView.DataContext = await CasinoViewModel.Load();
                break;
        }

        base.OnFrameworkInitializationCompleted();
    }
}