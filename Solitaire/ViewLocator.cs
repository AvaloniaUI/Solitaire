using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Solitaire.ViewModels;
using Solitaire.ViewModels.Pages;
using Solitaire.Views;
using Solitaire.Views.Pages;

namespace Solitaire;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
            return null;

        try
        {
            return data switch
            {
                TitleViewModel => new TitleView(),
                KlondikeSolitaireViewModel => new KlondikeSolitaireView(),
                FreeCellSolitaireViewModel => new FreeCellSolitaireView(),
                SpiderSolitaireViewModel => new SpiderSolitaireView(),
                GameStatisticsViewModel => new GameStatisticsView(),
                SettingsViewModel => new SettingsView(),
                StatisticsViewModel => new StatisticsView(),
                CasinoViewModel => new CasinoView(),
                _ => null
            };
        }
        catch (Exception ex)
        {
            return new TextBlock
            {
                Text = $"The view could not be created: {ex}"
            };
        }
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
