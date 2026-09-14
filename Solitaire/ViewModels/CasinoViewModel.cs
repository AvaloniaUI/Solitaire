using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Solitaire.Utils;
using Solitaire.ViewModels.Pages;

namespace Solitaire.ViewModels;

public partial class CasinoViewModel : ViewModelBase
{
    private ViewModelBase? _currentView;
    private int _navigationVersion;

    public ViewModelBase? CurrentView
    {
        get => _currentView;
        set
        {
            if (ReferenceEquals(_currentView, value))
                return;
            _navigationVersion++;
            if (_currentView is CardGameViewModel outgoing)
                outgoing.ResetForNavigation();
            if (value is CardGameViewModel incoming)
                incoming.ResetForNavigation();
            SetProperty(ref _currentView, value);
        }
    }

    internal void StartGame(CardGameViewModel game)
    {
        CurrentView = game;
        var version = _navigationVersion;
        Dispatcher.UIThread.Post(() =>
        {
            if (version == _navigationVersion && ReferenceEquals(CurrentView, game))
                game.NewGameCommand?.Execute(null);
        }, DispatcherPriority.Background);
    }



    public CasinoViewModel()
    {
        SettingsInstance = new SettingsViewModel(this);
        KlondikeInstance = new KlondikeSolitaireViewModel(this);
        SpiderInstance = new SpiderSolitaireViewModel(this);
        FreeCellInstance = new FreeCellSolitaireViewModel(this);
        TitleInstance = new TitleViewModel(this);
        StatisticsInstance = new StatisticsViewModel(this);
        CurrentView = TitleInstance;

    }
    public StatisticsViewModel StatisticsInstance { get; }

    public TitleViewModel TitleInstance { get; }
    public SettingsViewModel SettingsInstance { get; }
    public SpiderSolitaireViewModel SpiderInstance { get; }
    public FreeCellSolitaireViewModel FreeCellInstance { get; }
    public KlondikeSolitaireViewModel KlondikeInstance { get; }

    public async void Save()
    {
        await PlatformProviders.CasinoStorage.SaveObject(this, "mainSettings");
    }

    public static async Task<CasinoViewModel> CreateOrLoadFromDisk()
    {
        var ret = await PlatformProviders.CasinoStorage.LoadObject("mainSettings");
        if (ret is null)
            return new CasinoViewModel();

        // Refresh game logics.
        ret.FreeCellInstance.ResetGame();
        ret.KlondikeInstance.ResetGame();
        ret.SpiderInstance.ResetGame();
        return ret;
    }
}
