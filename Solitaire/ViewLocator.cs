using System;
using System.Collections;
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
            null => null,
            _ => new TextBlock
            {
                Text = $"View for {data.GetType().Name} wasn't found"
            }
        };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}