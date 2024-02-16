using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Solitaire.Models;
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
        StatisticsInstance = new StatisticsViewModel(this);
        CurrentView = TitleInstance;

        NavigateToTitleCommand = new RelayCommand(() =>
        {
            CurrentView = TitleInstance;
            Save();
        });
    }
    public StatisticsViewModel StatisticsInstance { get; }

    public TitleViewModel TitleInstance { get; }
    public SettingsViewModel SettingsInstance { get; }
    public SpiderSolitaireViewModel SpiderInstance { get; }
    public FreeCellSolitaireViewModel FreeCellInstance { get; }
    public KlondikeSolitaireViewModel KlondikeInstance { get; }

    public ICommand NavigateToTitleCommand { get; }

    /// <summary>
    /// Saves this instance.
    /// </summary>
    public async void Save()
    {
        var state = new PersistentState(
            SettingsInstance.GetState(),
            StatisticsInstance.KlondikeStatsInstance.GetState(),
            StatisticsInstance.SpiderStatsInstance.GetState(),
            StatisticsInstance.FreeCellStatsInstance.GetState());
        await PlatformProviders.CasinoStorage.SaveObject(state, "mainSettings");
    }

    /// <summary>
    /// Loads this instance.
    /// </summary>
    /// <returns></returns>
    public static async Task<CasinoViewModel> CreateOrLoadFromDisk()
    {
        var ret = new CasinoViewModel();
        var state = await PlatformProviders.CasinoStorage.LoadObject("mainSettings");
        if (state is not null)
        {
            ret.SettingsInstance.ApplyState(state.Settings);
            ret.StatisticsInstance.KlondikeStatsInstance.ApplyState(state.KlondikeStatsInstance);
            ret.StatisticsInstance.SpiderStatsInstance.ApplyState(state.SpiderStatsInstance);
            ret.StatisticsInstance.FreeCellStatsInstance.ApplyState(state.FreeCellStatsInstance);
        }
        return ret;
    }
}