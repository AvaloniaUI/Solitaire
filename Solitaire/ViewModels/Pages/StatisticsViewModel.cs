using System;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Solitaire.Models;
using Solitaire.Utils;

namespace Solitaire.ViewModels.Pages;

public partial class StatisticsViewModel : ViewModelBase
{
    private int _selection;
    public ICommand CycleStatisticsCommand { get; }
    public ICommand ResetSelectedStatsCommand { get; }
    public string SelectionName => _selection == 3 ? "Overall" : SelectedStatistics.GameName ?? "Statistics";
    public GameStatisticsViewModel SelectedStatistics => _selection switch
    {
        0 => KlondikeStatsInstance!,
        1 => FreeCellStatsInstance!,
        2 => SpiderStatsInstance!,
        _ => OverallStatistics
    };
    public GameStatisticsViewModel OverallStatistics { get; } = new("Overall");

    private void RefreshOverall()
    {
        var games = new[] { KlondikeStatsInstance!, FreeCellStatsInstance!, SpiderStatsInstance! };
        var overall = OverallStatistics;
        overall.GamesPlayed = games.Sum(game => game.GamesPlayed);
        overall.GamesWon = games.Sum(game => game.GamesWon);
        overall.GamesLost = games.Sum(game => game.GamesLost);
        overall.HighestScore = games.Max(game => game.HighestScore);
        overall.CumulativeScore = games.Sum(game => game.CumulativeScore);
        overall.AverageScore = overall.GamesWon == 0 ? 0 : overall.CumulativeScore / (double)overall.GamesWon;
        overall.CumulativeGameTime = TimeSpan.FromTicks(games.Sum(game => game.CumulativeGameTime.Ticks));
        overall.AverageGameTime = overall.GamesPlayed == 0 ? TimeSpan.Zero :
            TimeSpan.FromTicks(overall.CumulativeGameTime.Ticks / overall.GamesPlayed);
    }

    public ICommand NavigateToTitleCommand { get; }
    public ICommand ResetKlondikeStatsCommand { get; }
    public ICommand ResetFreeCellStatsCommand { get; }
    public ICommand ResetSpiderStatsCommand { get; }



    [ObservableProperty] private GameStatisticsViewModel? _klondikeStatsInstance;
    [ObservableProperty] private GameStatisticsViewModel? _spiderStatsInstance;
    [ObservableProperty] private GameStatisticsViewModel? _freeCellStatsInstance;


    public StatisticsViewModel(CasinoViewModel casinoViewModel)
    {
        CycleStatisticsCommand = new RelayCommand(() =>
        {
            _selection = (_selection + 1) % 4;
            OnPropertyChanged(nameof(SelectedStatistics));
            OnPropertyChanged(nameof(SelectionName));
        });
        ResetSelectedStatsCommand = new RelayCommand(() =>
        {
            if (_selection == 3)
            {
                KlondikeStatsInstance?.ResetCommand?.Execute(null);
                FreeCellStatsInstance?.ResetCommand?.Execute(null);
                SpiderStatsInstance?.ResetCommand?.Execute(null);
            }
            else
                SelectedStatistics.ResetCommand?.Execute(null);
        });
        var casinoViewModel1 = casinoViewModel;

        NavigateToTitleCommand = new RelayCommand(() =>
        {
            casinoViewModel1.CurrentView = casinoViewModel1.TitleInstance;
            PlatformProviders.CasinoStorage.SaveObject(casinoViewModel1, "mainSettings");
        });

        SpiderStatsInstance = new GameStatisticsViewModel(casinoViewModel.SpiderInstance);
        KlondikeStatsInstance = new GameStatisticsViewModel(casinoViewModel.KlondikeInstance);
        FreeCellStatsInstance = new GameStatisticsViewModel(casinoViewModel.FreeCellInstance);
        foreach (var game in new[] { KlondikeStatsInstance, FreeCellStatsInstance, SpiderStatsInstance })
            game.PropertyChanged += (_, _) => RefreshOverall();
        RefreshOverall();


        ResetKlondikeStatsCommand = new RelayCommand(() =>
        {
            KlondikeStatsInstance?.ResetCommand?.Execute(null);
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
