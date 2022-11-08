using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Solitaire.Models;
using Solitaire.Utils;
using System;

namespace Solitaire.ViewModels.Pages;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty] private Difficulty _difficulty = Difficulty.Easy;
    [ObservableProperty] private DrawMode _drawMode = DrawMode.DrawOne;
    [ObservableProperty] private string _drawModeText;
    
    public ICommand NavigateToTitleCommand { get; }
    public ICommand ResetKlondikeStatsCommand { get; }
    public ICommand ResetSpiderStatsCommand { get; }
    
    public ICommand DrawModeCommand { get; }
    public ICommand DrawModeOneCommand { get; }
    public ICommand DrawModeThreeCommand { get; }

    public SettingsViewModel(CasinoViewModel casinoViewModel)
    {
        var casinoViewModel1 = casinoViewModel;

        NavigateToTitleCommand = new RelayCommand(() =>
        {
            casinoViewModel1.CurrentView = casinoViewModel1.TitleInstance;
            PlatformProviders.CasinoStorage.SaveObject(casinoViewModel1, "mainSettings");
        });
        ResetKlondikeStatsCommand = new RelayCommand(() =>
        {
            casinoViewModel1.TitleInstance.KlondikeStatsInstance?.ResetCommand?.Execute(null);
        });
        ResetSpiderStatsCommand = new RelayCommand(() =>
        {
            casinoViewModel1.TitleInstance.SpiderStatsInstance?.ResetCommand?.Execute(null);
        });

        DrawModeCommand = new RelayCommand(() =>
        {
            if (DrawMode == DrawMode.DrawOne)
            {
                DrawMode = DrawMode.DrawThree;
            }
            else
            {
                DrawMode = DrawMode.DrawOne;
            }
        });

        this.WhenAnyValue(x => x.DrawMode)
            .Subscribe(x =>
            {
                DrawModeText = $"Draw Mode: {DrawMode}";
            });

    }
}