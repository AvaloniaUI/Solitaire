using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Solitaire.ViewModels.Pages;

public partial class TitleViewModel : ViewModelBase
{
    
#if DEBUG
    public TitleViewModel() { }
#endif
    
    public ICommand? NavigateToKlondikeCommand { get; }

    public ICommand? NavigateToSpiderCommand { get; }
    
    public ICommand? NavigateToFreeCellCommand { get; }
    
    public ICommand? NavigateToStatisticsCommand { get; }

    public ICommand? NavigateToSettingsCommand { get; }

    public TitleViewModel(CasinoViewModel casinoViewModel)
    {
 
        NavigateToKlondikeCommand = new RelayCommand(() =>
        {
            casinoViewModel.CurrentView = casinoViewModel.KlondikeInstance;
            Dispatcher.UIThread.Post(() =>
            {
                casinoViewModel.KlondikeInstance.NewGameCommand?.Execute(default);
            }, DispatcherPriority.Background);
        });

        NavigateToSpiderCommand = new RelayCommand(() =>
        {
            casinoViewModel.CurrentView = casinoViewModel.SpiderInstance;
            
            Dispatcher.UIThread.Post(() =>
            {
                casinoViewModel.SpiderInstance.NewGameCommand?.Execute(default);
            }, DispatcherPriority.Background);
         });

        NavigateToFreeCellCommand = new RelayCommand(() =>
        {
            casinoViewModel.CurrentView = casinoViewModel.FreeCellInstance;
            
            Dispatcher.UIThread.Post(() =>
            {
                casinoViewModel.FreeCellInstance.NewGameCommand?.Execute(default);
            }, DispatcherPriority.Background);
         });

        NavigateToSettingsCommand = new RelayCommand(() =>
        {
            casinoViewModel.CurrentView = casinoViewModel.SettingsInstance;
        });

        NavigateToStatisticsCommand = new RelayCommand(() =>
        {
            casinoViewModel.CurrentView = casinoViewModel.StatisticsInstance;
        });
        
    }
}