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
        var dataContext = await CasinoViewModel.CreateOrLoadFromDisk();

        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.MainWindow = new MainWindow()
                {
                    DataContext = dataContext
                };
                break;
            case ISingleViewApplicationLifetime singleViewPlatform:
                singleViewPlatform.MainView = new CasinoView()
                {
                    DataContext = dataContext
                };
                break;
        }

        base.OnFrameworkInitializationCompleted();
    }
}