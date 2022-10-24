﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Solitaire.Utils;

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
        KlondikeInstance = new KlondikeSolitaireViewModel(this);
        SpiderInstance = new SpiderSolitaireViewModel(this);
        SettingsInstance = new SettingsViewModel(this);
        TitleInstance = new TitleViewModel(this);
        CurrentView = TitleInstance;
    }

    public TitleViewModel TitleInstance { get; }
    public SettingsViewModel SettingsInstance { get; }
    public SpiderSolitaireViewModel SpiderInstance { get; }
    public KlondikeSolitaireViewModel KlondikeInstance { get; }

    /// <summary>
    /// Saves this instance.
    /// </summary>
    public async void Save()
    {
        await PlatformProviders.CasinoStorage.SaveObject(this);
    }

    /// <summary>
    /// Loads this instance.
    /// </summary>
    /// <returns></returns>
    public static async Task<CasinoViewModel> Load()
    {
        var ret = await PlatformProviders.CasinoStorage.LoadObject();
        if (ret is null) return new CasinoViewModel();
        // Refresh game logics.
        ret.KlondikeInstance.ResetGame();
        ret.SpiderInstance.ResetGame();
        return ret;
    }
}