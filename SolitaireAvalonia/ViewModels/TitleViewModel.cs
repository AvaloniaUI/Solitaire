using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SolitaireAvalonia.ViewModels;

public partial class TitleViewModel : ViewModelBase
{
    private readonly CasinoViewModel _casinoViewModel;

    [ObservableProperty] private GameStatisticsViewModel _klondikeStatsInstance;
    [ObservableProperty] private GameStatisticsViewModel _spiderStatsInstance;

    public ICommand NavigateToKlondikeCommand { get; }

    public ICommand NavigateToSpiderCommand { get; }

    public ICommand NavigateToSettingsCommand { get; }

    public TitleViewModel(CasinoViewModel casinoViewModel)
    {
        _casinoViewModel = casinoViewModel;

        NavigateToKlondikeCommand = new RelayCommand(() =>
        {
            _casinoViewModel.CurrentView = _casinoViewModel.KlondikeInstance;
        });

        NavigateToSpiderCommand = new RelayCommand(() =>
        {
            _casinoViewModel.CurrentView = _casinoViewModel.SpiderInstance;
        });

        NavigateToSettingsCommand = new RelayCommand(() =>
        {
            _casinoViewModel.CurrentView = _casinoViewModel.SettingsInstance;
        });


        SpiderStatsInstance = new GameStatisticsViewModel(casinoViewModel.SpiderInstance);
        KlondikeStatsInstance = new GameStatisticsViewModel(casinoViewModel.KlondikeInstance);
    }
}