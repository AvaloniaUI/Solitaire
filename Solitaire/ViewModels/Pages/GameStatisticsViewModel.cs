using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Solitaire.ViewModels.Pages;

/// <summary>
/// A set of general statistics for a game.
/// </summary>
public partial class GameStatisticsViewModel : ViewModelBase
{
#if DEBUG
    public GameStatisticsViewModel()
    {
    }
#endif

    public GameStatisticsViewModel(CardGameViewModel cardGameInstance)
    {
        _cardGameInstance = cardGameInstance;
        GameName = cardGameInstance.GameName;
        cardGameInstance.RegisterStatsInstance(this);
        ResetCommand = new RelayCommand(DoReset);
    }

    internal GameStatisticsViewModel(string gameName)
    {
        GameName = gameName;
        ShowStreaks = false;
    }

    public bool ShowStreaks { get; } = true;

    public ICommand? ResetCommand { get; }

    private void DoReset()
    {
        GamesPlayed = 0;
        GamesWon = 0;
        GamesLost = 0;
        HighestWinningStreak = 0;
        HighestLosingStreak = 0;
        CurrentStreak = 0;
        CumulativeScore = 0;
        HighestScore = 0;
        AverageScore = 0;
        CumulativeGameTime = TimeSpan.FromSeconds(0);
        AverageGameTime = TimeSpan.FromSeconds(0);
    }

    public void UpdateStatistics()
    {
        GamesPlayed++;
        if (_cardGameInstance?.IsGameWon ?? false)
            GamesWon++;
        else
            GamesLost++;

        if (_cardGameInstance?.IsGameWon ?? false)
            CurrentStreak = CurrentStreak < 0 ? 1 : CurrentStreak + 1;
        else
            CurrentStreak = CurrentStreak > 0 ? -1 : CurrentStreak - 1;

        if (CurrentStreak > HighestWinningStreak)
            HighestWinningStreak = CurrentStreak;
        else if (Math.Abs(CurrentStreak) > HighestLosingStreak)
            HighestLosingStreak = Math.Abs(CurrentStreak);

        if (_cardGameInstance?.Score > HighestScore)
            HighestScore = _cardGameInstance.Score;

        // Only wins contribute to the average score.
        if (_cardGameInstance?.IsGameWon ?? false)
        {
            CumulativeScore += _cardGameInstance.Score;
            AverageScore = CumulativeScore / (double)GamesWon;
        }

        CumulativeGameTime += _cardGameInstance?.ElapsedTime ?? TimeSpan.Zero;
        AverageGameTime = TimeSpan.FromTicks(CumulativeGameTime.Ticks / (GamesWon + GamesLost));
    }

    public string? GameName { get; }

    [ObservableProperty] private int _gamesPlayed;
    [ObservableProperty] private int _gamesWon;
    [ObservableProperty] private int _gamesLost;
    [ObservableProperty] private int _highestWinningStreak;
    [ObservableProperty] private int _highestLosingStreak;
    [ObservableProperty] private int _currentStreak;
    [ObservableProperty] private int _cumulativeScore;
    [ObservableProperty] private int _highestScore;
    [ObservableProperty] private double _averageScore;
    [ObservableProperty] private TimeSpan _cumulativeGameTime;
    [ObservableProperty] private TimeSpan _averageGameTime;
    private readonly CardGameViewModel? _cardGameInstance;
}
