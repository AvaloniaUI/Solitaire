using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Solitaire.Models;
using Solitaire.Utils;

namespace Solitaire.ViewModels.Pages;

public partial class StatisticsViewModel : ViewModelBase
{
    public ICommand NavigateToTitleCommand { get; }
    public ICommand ResetKlondikeStatsCommand { get; }
    public ICommand ResetFreeCellStatsCommand { get; }
    public ICommand ResetSpiderStatsCommand { get; }

     

    [ObservableProperty] private GameStatisticsViewModel? _klondikeStatsInstance;
    [ObservableProperty] private GameStatisticsViewModel? _spiderStatsInstance;
    [ObservableProperty] private GameStatisticsViewModel? _freeCellStatsInstance;


    public StatisticsViewModel(CasinoViewModel casinoViewModel)
    {
        var casinoViewModel1 = casinoViewModel;

        NavigateToTitleCommand = new RelayCommand(() =>
        {
            casinoViewModel1.CurrentView = casinoViewModel1.TitleInstance;
            PlatformProviders.CasinoStorage.SaveObject(casinoViewModel1, "mainSettings");
        });
        
        SpiderStatsInstance = new GameStatisticsViewModel(casinoViewModel.SpiderInstance);
        KlondikeStatsInstance = new GameStatisticsViewModel(casinoViewModel.KlondikeInstance);
        FreeCellStatsInstance = new GameStatisticsViewModel(casinoViewModel.FreeCellInstance);
        
        
        ResetKlondikeStatsCommand = new RelayCommand(() =>
        {
            SpiderStatsInstance?.ResetCommand?.Execute(null);
        });
        
        ResetSpiderStatsCommand = new RelayCommand(() =>
        {
            SpiderStatsInstance?.ResetCommand?.Execute(null);
        });
        
        ResetFreeCellStatsCommand = new RelayCommand(() =>
        {
            FreeCellStatsInstance?.ResetCommand?.Execute(null);
        });
    }
}