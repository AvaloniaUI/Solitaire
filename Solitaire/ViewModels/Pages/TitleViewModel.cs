using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Solitaire.ViewModels.Pages;

public partial class TitleViewModel : ViewModelBase
{
    [ObservableProperty] private GameStatisticsViewModel? _klondikeStatsInstance;
    [ObservableProperty] private GameStatisticsViewModel? _spiderStatsInstance;

    
#if DEBUG
    public TitleViewModel() { }
#endif
    
    public ICommand? NavigateToKlondikeCommand { get; }

    public ICommand? NavigateToSpiderCommand { get; }

    public ICommand? NavigateToSettingsCommand { get; }

    public TitleViewModel(CasinoViewModel casinoViewModel)
    {
 
        NavigateToKlondikeCommand = new RelayCommand(() =>
        {
            casinoViewModel.CurrentView = casinoViewModel.KlondikeInstance;
            casinoViewModel.KlondikeInstance?.NewGameCommand?.Execute(default);
        });

        NavigateToSpiderCommand = new RelayCommand(() =>
        {
            casinoViewModel.CurrentView = casinoViewModel.SpiderInstance;
            casinoViewModel.SpiderInstance?.NewGameCommand?.Execute(default);
        });

        NavigateToSettingsCommand = new RelayCommand(() =>
        {
            casinoViewModel.CurrentView = casinoViewModel.SettingsInstance;
        });
        
        SpiderStatsInstance = new GameStatisticsViewModel(casinoViewModel.SpiderInstance!);
        KlondikeStatsInstance = new GameStatisticsViewModel(casinoViewModel.KlondikeInstance!);
    }
}