﻿using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Solitaire.Utils;
using Solitaire.ViewModels.Pages;

namespace Solitaire.ViewModels;

/// <summary>
/// The casino view model.
/// </summary>
public partial class CasinoViewModel : ViewModelBase
{
    [ObservableProperty] private ViewModelBase? _currentView;

    /// <summary>
    /// Initializes a new instance of the <see cref="CasinoViewModel"/> class.
    /// </summary>
    public CasinoViewModel()
    {
        SettingsInstance = new SettingsViewModel(this);
        KlondikeInstance = new KlondikeSolitaireViewModel(this);
        SpiderInstance = new SpiderSolitaireViewModel(this);
        FreeCellInstance = new FreeCellSolitaireViewModel(this);
        TitleInstance = new TitleViewModel(this);
        CurrentView = TitleInstance;
    }

    public TitleViewModel TitleInstance { get; }
    public SettingsViewModel SettingsInstance { get; }
    public SpiderSolitaireViewModel SpiderInstance { get; }
    public FreeCellSolitaireViewModel FreeCellInstance { get; }
    public KlondikeSolitaireViewModel KlondikeInstance { get; }

    /// <summary>
    /// Saves this instance.
    /// </summary>
    public async void Save()
    {
        await PlatformProviders.CasinoStorage.SaveObject(this, "mainSettings");
    }

    /// <summary>
    /// Loads this instance.
    /// </summary>
    /// <returns></returns>
    public static async Task<CasinoViewModel> CreateOrLoadFromDisk()
    {
        var ret = await PlatformProviders.CasinoStorage.LoadObject("mainSettings");
        if (ret is null) return new CasinoViewModel();

        // Refresh game logics.
        ret.FreeCellInstance.ResetGame();
        ret.KlondikeInstance.ResetGame();
        ret.SpiderInstance.ResetGame();
        return ret;
    }
}