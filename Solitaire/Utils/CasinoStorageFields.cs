using Solitaire.ViewModels;
using Solitaire.ViewModels.Pages;

namespace Solitaire.Utils;

internal static class CasinoStorageFields
{
    public static void Transfer(CasinoViewModel casino, StorageJsonFields fields)
    {
        fields.Object(nameof(casino.SettingsInstance), data => Settings(casino.SettingsInstance, data));
        fields.Object(nameof(casino.StatisticsInstance), data =>
        {
            var statistics = casino.StatisticsInstance;
            data.Object(nameof(statistics.KlondikeStatsInstance), game => Statistics(statistics.KlondikeStatsInstance!, game));
            data.Object(nameof(statistics.FreeCellStatsInstance), game => Statistics(statistics.FreeCellStatsInstance!, game));
            data.Object(nameof(statistics.SpiderStatsInstance), game => Statistics(statistics.SpiderStatsInstance!, game));
        });
    }

    private static void Settings(SettingsViewModel model, StorageJsonFields data)
    {
        var types = StorageJsonContext.Default;
        data.EnumValue(nameof(model.Difficulty), () => model.Difficulty, value => model.Difficulty = value, types.Difficulty);
        data.EnumValue(nameof(model.DrawMode), () => model.DrawMode, value => model.DrawMode = value, types.DrawMode);
        data.Value(nameof(model.LightDirection), () => model.LightDirection, value => model.LightDirection = value, types.Double);
        data.Value(nameof(model.LightElevation), () => model.LightElevation, value => model.LightElevation = value, types.Double);
    }

    private static void Statistics(GameStatisticsViewModel model, StorageJsonFields data)
    {
        var types = StorageJsonContext.Default;
        data.Value(nameof(model.GamesPlayed), () => model.GamesPlayed, value => model.GamesPlayed = value, types.Int32);
        data.Value(nameof(model.GamesWon), () => model.GamesWon, value => model.GamesWon = value, types.Int32);
        data.Value(nameof(model.GamesLost), () => model.GamesLost, value => model.GamesLost = value, types.Int32);
        data.Value(nameof(model.HighestWinningStreak), () => model.HighestWinningStreak, value => model.HighestWinningStreak = value, types.Int32);
        data.Value(nameof(model.HighestLosingStreak), () => model.HighestLosingStreak, value => model.HighestLosingStreak = value, types.Int32);
        data.Value(nameof(model.CurrentStreak), () => model.CurrentStreak, value => model.CurrentStreak = value, types.Int32);
        data.Value(nameof(model.CumulativeScore), () => model.CumulativeScore, value => model.CumulativeScore = value, types.Int32);
        data.Value(nameof(model.HighestScore), () => model.HighestScore, value => model.HighestScore = value, types.Int32);
        data.Value(nameof(model.AverageScore), () => model.AverageScore, value => model.AverageScore = value, types.Double);
        data.Value(nameof(model.CumulativeGameTime), () => model.CumulativeGameTime, value => model.CumulativeGameTime = value, types.TimeSpan);
        data.Value(nameof(model.AverageGameTime), () => model.AverageGameTime, value => model.AverageGameTime = value, types.TimeSpan);
    }
}
