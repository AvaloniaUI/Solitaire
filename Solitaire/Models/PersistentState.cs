using System;

namespace Solitaire.Models;

public record SettingsState(
    Difficulty Difficulty,
    DrawMode DrawMode);

public record GameStatisticsState(
    int GamesPlayed,
    int GamesWon,
    int GamesLost,
    int HighestWinningStreak,
    int HighestLosingStreak,
    int CurrentStreak,
    int CumulativeScore,
    int HighestScore,
    double AverageScore,
    TimeSpan CumulativeGameTime,
    TimeSpan AverageGameTime);

public record PersistentState(
    SettingsState Settings,
    GameStatisticsState KlondikeStatsInstance,
    GameStatisticsState SpiderStatsInstance,
    GameStatisticsState FreeCellStatsInstance);