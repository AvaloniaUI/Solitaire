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
    [ObservableProperty] private string? _drawModeText;
    [ObservableProperty] private string? _difficultyText;
    public ICommand NavigateToTitleCommand { get; }
    
    public ICommand DrawModeCommand { get; } 

    public ICommand DifficultyCommand { get; } 

    
    public SettingsViewModel(CasinoViewModel casinoViewModel)
    {
        var casinoViewModel1 = casinoViewModel;

        NavigateToTitleCommand = new RelayCommand(() =>
        {
            casinoViewModel1.CurrentView = casinoViewModel1.TitleInstance;
            casinoViewModel1.Save();
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
                Difficulty.Hard => Difficulty.Easy,
                _ => throw new ArgumentOutOfRangeException()
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

    public void ApplyState(SettingsState state)
    {
        Difficulty = state.Difficulty;
        DrawMode = state.DrawMode;
    }

    public SettingsState GetState()
    {
        return new SettingsState(Difficulty, DrawMode);
    }
}