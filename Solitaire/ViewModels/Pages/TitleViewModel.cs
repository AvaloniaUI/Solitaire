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

        NavigateToKlondikeCommand = new RelayCommand(() => casinoViewModel.StartGame(casinoViewModel.KlondikeInstance));
        NavigateToSpiderCommand = new RelayCommand(() => casinoViewModel.StartGame(casinoViewModel.SpiderInstance));
        NavigateToFreeCellCommand = new RelayCommand(() => casinoViewModel.StartGame(casinoViewModel.FreeCellInstance));

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
