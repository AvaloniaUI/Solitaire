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
    [ObservableProperty] private string _drawModeText = string.Empty;
    [ObservableProperty] private string _difficultyText = string.Empty;
    private double _lightDirection = TableLighting.DefaultDirection;
    private double _lightElevation = TableLighting.DefaultElevation;

    public double LightDirection
    {
        get => _lightDirection;
        set
        {
            var direction = double.IsFinite(value) ? Math.Clamp(value, 0, 360) : TableLighting.DefaultDirection;
            if (!SetProperty(ref _lightDirection, direction))
                return;
            OnPropertyChanged(nameof(LightDirectionText));
            TableLighting.Set(direction, LightElevation);
        }
    }

    public double LightElevation
    {
        get => _lightElevation;
        set
        {
            var elevation = double.IsFinite(value) ? Math.Clamp(value, 20, 80) : TableLighting.DefaultElevation;
            if (!SetProperty(ref _lightElevation, elevation))
                return;
            OnPropertyChanged(nameof(LightElevationText));
            TableLighting.Set(LightDirection, elevation);
        }
    }

    public string LightDirectionText => new[] { "Above", "Upper right", "Right", "Lower right", "Below", "Lower left", "Left", "Upper left" }[(int)Math.Round(LightDirection / 45) % 8];
    public string LightElevationText => LightElevation < 42.5 ? "Low" : LightElevation < 65 ? "Medium" : "High";
    public ICommand ResetLightCommand { get; }

    public ICommand NavigateToTitleCommand { get; }

    public ICommand DrawModeCommand { get; }

    public ICommand DifficultyCommand { get; }


    public SettingsViewModel(CasinoViewModel casinoViewModel)
    {
        TableLighting.Set(LightDirection, LightElevation);
        ResetLightCommand = new RelayCommand(() =>
        {
            LightDirection = TableLighting.DefaultDirection;
            LightElevation = TableLighting.DefaultElevation;
        });
        var casinoViewModel1 = casinoViewModel;

        NavigateToTitleCommand = new RelayCommand(() =>
        {
            casinoViewModel1.CurrentView = casinoViewModel1.TitleInstance;
            PlatformProviders.CasinoStorage.SaveObject(casinoViewModel1, "mainSettings");
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
                _ => throw new System.Runtime.CompilerServices.SwitchExpressionException(Difficulty)
            };
        });

        this.ObserveProperty(nameof(SettingsViewModel.DrawMode), static x => x.DrawMode)
            .Subscribe(x =>
            {
                DrawModeText = $"{DrawMode.ToString()
                    .Replace("Draw", "")} Card{(DrawMode == DrawMode.DrawThree ? "s" : "")}";
            });

        this.ObserveProperty(nameof(SettingsViewModel.Difficulty), static x => x.Difficulty)
            .Subscribe(x =>
            {
                DifficultyText = $"{Difficulty}";
            });

    }
}
