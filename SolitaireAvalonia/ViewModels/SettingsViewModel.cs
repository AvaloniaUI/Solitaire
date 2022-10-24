using System.ComponentModel;
using System.Reactive.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using SolitaireAvalonia.Utils;

namespace SolitaireAvalonia.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly CasinoViewModel _casinoViewModel;
    
    [ObservableProperty] private Difficulty _difficulty = Difficulty.Easy;
    [ObservableProperty] private DrawMode _drawMode = DrawMode.DrawOne;
    
    public ICommand NavigateToTitleCommand { get; }
    public ICommand ResetKlondikeStatsCommand { get; }
    public ICommand ResetSpiderStatsCommand { get; }
    public SettingsViewModel(CasinoViewModel casinoViewModel)
    {
        _casinoViewModel = casinoViewModel;

        NavigateToTitleCommand = new RelayCommand(() =>
        {
            _casinoViewModel.CurrentView = _casinoViewModel.TitleInstance;
            PlatformProviders.CasinoStorage.SaveObject(_casinoViewModel);
        });
        ResetKlondikeStatsCommand = new RelayCommand(() =>
        {
            _casinoViewModel.TitleInstance.KlondikeStatsInstance.ResetCommand.Execute(null);
        });
        ResetSpiderStatsCommand = new RelayCommand(() =>
        {
            _casinoViewModel.TitleInstance.SpiderStatsInstance.ResetCommand.Execute(null);
        });

 
         
    }
}