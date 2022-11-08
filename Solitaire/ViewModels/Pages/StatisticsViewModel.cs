using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Solitaire.Models;
using Solitaire.Utils;

namespace Solitaire.ViewModels.Pages;

public partial class StatisticsViewModel : ViewModelBase
{
    [ObservableProperty] private Difficulty _difficulty = Difficulty.Easy;
    [ObservableProperty] private DrawMode _drawMode = DrawMode.DrawOne;
    [ObservableProperty] private string _drawModeText;
    [ObservableProperty] private string _difficultyText;
    public ICommand NavigateToTitleCommand { get; }
    public ICommand ResetKlondikeStatsCommand { get; }
    public ICommand ResetSpiderStatsCommand { get; }
    
    public ICommand DrawModeCommand { get; } 

    public ICommand DifficultyCommand { get; } 

    
    public StatisticsViewModel(CasinoViewModel casinoViewModel)
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
            DrawMode = DrawMode == DrawMode.DrawOne ? DrawMode.DrawThree : DrawMode.DrawOne;
        });


        DifficultyCommand = new RelayCommand(() =>
        {
            Difficulty = Difficulty switch
            {
                Difficulty.Easy => Difficulty.Medium,
                Difficulty.Medium => Difficulty.Hard,
                Difficulty.Hard => Difficulty.Easy
            };
        });
        
        this.WhenAnyValue(x => x.DrawMode)
            .Subscribe(x =>
            {
                DrawModeText = $"{DrawMode.ToString()
                    .Replace("Draw", "")} Card{(DrawMode == DrawMode.DrawThree? "s" : "")}" ;
            });

        this.WhenAnyValue(x => x.Difficulty)
            .Subscribe(x =>
            {
                DifficultyText = $"{Difficulty}";
            });

    }
}