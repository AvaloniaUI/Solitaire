using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Solitaire.Models;
using Solitaire.Utils;

namespace Solitaire.ViewModels.Pages;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty] private Difficulty _difficulty = Difficulty.Easy;
    [ObservableProperty] private DrawMode _drawMode = DrawMode.DrawOne;
    
    public ICommand NavigateToTitleCommand { get; }
    public ICommand ResetKlondikeStatsCommand { get; }
    public ICommand ResetSpiderStatsCommand { get; }
    public SettingsViewModel(CasinoViewModel casinoViewModel)
    {
        var casinoViewModel1 = casinoViewModel;

        NavigateToTitleCommand = new RelayCommand(() =>
        {
            casinoViewModel1.CurrentView = casinoViewModel1.TitleInstance;
            PlatformProviders.CasinoStorage.SaveObject(casinoViewModel1, "mainSettings");
        });
        ResetKlondikeStatsCommand = new RelayCommand(() =>
        {
            casinoViewModel1.TitleInstance.KlondikeStatsInstance?.ResetCommand?.Execute(null);
        });
        ResetSpiderStatsCommand = new RelayCommand(() =>
        {
            casinoViewModel1.TitleInstance.SpiderStatsInstance?.ResetCommand?.Execute(null);
        });

 
         
    }
}