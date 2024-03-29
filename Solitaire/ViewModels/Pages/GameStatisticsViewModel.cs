﻿using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Solitaire.Models;

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
        //  Create the reset command.
        ResetCommand = new RelayCommand(DoReset);
    }

    public ICommand? ResetCommand { get; }

    /// <summary>
    /// Resets the statistics.
    /// </summary>
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
        //  Update the games won or lost.
        GamesPlayed++;
        if (_cardGameInstance?.IsGameWon ?? false)
            GamesWon++;
        else
            GamesLost++;

        //  Update the current streak.
        if (_cardGameInstance?.IsGameWon ?? false)
            CurrentStreak = CurrentStreak < 0 ? 1 : CurrentStreak + 1;
        else
            CurrentStreak = CurrentStreak > 0 ? -1 : CurrentStreak - 1;

        //  Update the highest streaks.
        if (CurrentStreak > HighestWinningStreak)
            HighestWinningStreak = CurrentStreak;
        else if (Math.Abs(CurrentStreak) > HighestLosingStreak)
            HighestLosingStreak = Math.Abs(CurrentStreak);

        //  Update the highest score.
        if (_cardGameInstance?.Score > HighestScore)
            HighestScore = _cardGameInstance.Score;

        //  Update the average score. Only won games
        //  contribute to the running average.
        if (_cardGameInstance?.IsGameWon ?? false)
        {
            CumulativeScore += _cardGameInstance.Score;
            AverageScore = CumulativeScore / (double) GamesWon;
        }

        //  Update the average game time.
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

    public void ApplyState(GameStatisticsState state)
    {
        GamesPlayed = state.GamesPlayed;
        GamesWon = state.GamesWon;
        GamesLost = state.GamesLost;
        HighestWinningStreak = state.HighestWinningStreak;
        HighestLosingStreak = state.HighestLosingStreak;
        CurrentStreak = state.CurrentStreak;
        CumulativeScore = state.CumulativeScore;
        HighestScore = state.HighestScore;
        AverageScore = state.AverageScore;
        CumulativeGameTime = state.CumulativeGameTime;
        AverageGameTime = state.AverageGameTime;
    }

    public GameStatisticsState GetState()
    {
        return new GameStatisticsState(
            GamesPlayed,
            GamesWon,
            GamesLost,
            HighestWinningStreak,
            HighestLosingStreak,
            CurrentStreak,
            CumulativeScore,
            HighestScore,
            AverageScore,
            CumulativeGameTime,
            AverageGameTime
        );
    }
}