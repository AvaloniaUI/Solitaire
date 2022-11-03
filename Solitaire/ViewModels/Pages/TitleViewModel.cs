using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Solitaire.ViewModels.Pages;

public partial class TitleViewModel : ViewModelBase
{
    [ObservableProperty] private GameStatisticsViewModel? _klondikeStatsInstance;
    [ObservableProperty] private GameStatisticsViewModel? _spiderStatsInstance;
    [ObservableProperty] private GameStatisticsViewModel? _freeCellStatsInstance;

    
#if DEBUG
    public TitleViewModel() { }
#endif
    
    public ICommand? NavigateToKlondikeCommand { get; }

    public ICommand? NavigateToSpiderCommand { get; }
    
    public ICommand? NavigateToFreeCellCommand { get; }

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
        
        SpiderStatsInstance = new GameStatisticsViewModel(casinoViewModel.SpiderInstance);
        KlondikeStatsInstance = new GameStatisticsViewModel(casinoViewModel.KlondikeInstance);
        FreeCellStatsInstance = new GameStatisticsViewModel(casinoViewModel.FreeCellInstance);
    }
}