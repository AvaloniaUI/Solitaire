using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SolitaireAvalonia.ViewModels;

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

    //
    // /// <summary>
    // /// Initialises this instance.
    // /// </summary>
    // public void Initialise()
    // {
    //     //  We're going to listen out for certain commands in the game
    //     //  so that we can keep track of scores etc.
    //     KlondikeSolitaireViewModel.DealNewGameCommand.Executing += new CancelCommandEventHandler(KlondikeDealNewGameCommand_Executing);
    //     KlondikeSolitaireViewModel.GameWon += new Action(KlondikeSolitaireViewModel_GameWon);
    //     KlondikeSolitaireViewModel.GoToCasinoCommand.Executed += new CommandEventHandler(GoToCasinoCommand_Executed);
    //     SpiderSolitaireViewModel.DealNewGameCommand.Executing += new CancelCommandEventHandler(SpiderDealNewGameCommand_Executing);
    //     SpiderSolitaireViewModel.GameWon += new Action(SpiderSolitaireViewModel_GameWon);
    //     SpiderSolitaireViewModel.GoToCasinoCommand.Executed +=new CommandEventHandler(GoToCasinoCommand_Executed);
    //
    //     //  Set the deck we're using for brushes.
    //     PlayingCardToBrushConverter.SetDeckFolder(DeckFolder);
    // }
    //
    // /// <summary>
    // /// Called when Klondike is won.
    // /// </summary>
    // void KlondikeSolitaireViewModel_GameWon()
    // {
    //     //  The game was won, update the stats.
    //     KlondikeSolitaireStatistics.UpdateStatistics(KlondikeSolitaireViewModel);
    //     Save();
    // }
    //
    // /// <summary>
    // /// Called when Spider is won.
    // /// </summary>
    // void SpiderSolitaireViewModel_GameWon()
    // {
    //     //  The game was won, update the stats.
    //     SpiderSolitaireStatistics.UpdateStatistics(KlondikeSolitaireViewModel);
    //     Save();
    // }
    //
    // /// <summary>
    // /// Handles the Executing event of the DealNewGameCommand control.
    // /// </summary>
    // /// <param name="sender">The source of the event.</param>
    // /// <param name="args">The <see cref="Apex.MVVM.CommandEventArgs"/> instance containing the event data.</param>
    // void KlondikeDealNewGameCommand_Executing(object sender, CancelCommandEventArgs args)
    // {
    //     //  If we've made any moves, update the stats.
    //     if (KlondikeSolitaireViewModel.Moves > 0)
    //         KlondikeSolitaireStatistics.UpdateStatistics(KlondikeSolitaireViewModel);
    //     Save();
    // }
    //
    // /// <summary>
    // /// Handles the Executed event of the DealNewGameCommand control.
    // /// </summary>
    // /// <param name="sender">The source of the event.</param>
    // /// <param name="args">The <see cref="Apex.MVVM.CommandEventArgs"/> instance containing the event data.</param>
    // void SpiderDealNewGameCommand_Executing(object sender, CancelCommandEventArgs args)
    // {
    //     //  If we've made any moves, update the stats.
    //     if (SpiderSolitaireViewModel.Moves > 0)
    //         SpiderSolitaireStatistics.UpdateStatistics(SpiderSolitaireViewModel);
    //     Save();
    // }
    //
    // /// <summary>
    // /// Handles the Executed event of the GoToCasinoCommand control.
    // /// </summary>
    // /// <param name="sender">The source of the event.</param>
    // /// <param name="args">The <see cref="Apex.MVVM.CommandEventArgs"/> instance containing the event data.</param>
    // void GoToCasinoCommand_Executed(object sender, CommandEventArgs args)
    // {
    //     GoToCasinoCommand.DoExecute(null);
    // }
    //
    // /// <summary>
    // /// Saves this instance.
    // /// </summary>
    // public void Save()
    // {
    //     // Get a new isolated store for this user, domain, and assembly.
    //     IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User |
    //         IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null);
    //
    //     //  Create data stream.
    //     using (IsolatedStorageFileStream isoStream =
    //         new IsolatedStorageFileStream("Casino.xml", FileMode.Create, isoStore))
    //     {
    //         XmlSerializer casinoSerializer = new XmlSerializer(typeof(CasinoViewModel));
    //         casinoSerializer.Serialize(isoStream, this);
    //     }
    // }
    //
    // /// <summary>
    // /// Loads this instance.
    // /// </summary>
    // /// <returns></returns>
    // public static CasinoViewModel Load()
    // {
    //     // Get a new isolated store for this user, domain, and assembly.
    //     IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User |
    //         IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null);
    //
    //     //  Create data stream.
    //     try
    //     {
    //         //  Save the casino.
    //         using (IsolatedStorageFileStream isoStream =
    //             new IsolatedStorageFileStream("Casino.xml", FileMode.Open, isoStore))
    //         {
    //             XmlSerializer casinoSerializer = new XmlSerializer(typeof(CasinoViewModel));
    //             return (CasinoViewModel)casinoSerializer.Deserialize(isoStream);
    //         }
    //     }
    //     catch
    //     {
    //     }
    //
    //     return new CasinoViewModel();
    // }
    //
    // /// <summary>
    // /// Goes to the casino.
    // /// </summary>
    // private void DoGoToCasino()
    // {
    //     KlondikeSolitaireViewModel.StopTimer();
    //     SpiderSolitaireViewModel.StopTimer();
    // }
    //
    // /// <summary>
    // /// Goes to spider.
    // /// </summary>
    // private void DoGoToSpiderSolitaire()
    // {
    //     if(SpiderSolitaireViewModel.Moves > 0)
    //         SpiderSolitaireViewModel.StartTimer();
    // }
    //
    // /// <summary>
    // /// Goes to Klondike.
    // /// </summary>
    // private void DoGoToKlondikeSolitaire()
    // {
    //     if(KlondikeSolitaireViewModel.Moves > 0)
    //         KlondikeSolitaireViewModel.StartTimer();
    // }
    //
    // /// <summary>
    // /// The settings command.
    // /// </summary>
    // private void DoSettingsCommand()
    // {
    // }
        
        
    // /// <summary>
    // /// The Klondike stats.
    // /// </summary>
    // private NotifyingProperty KlondikeSolitaireStatisticsProperty =
    //   new NotifyingProperty("KlondikeSolitaireStatistics", typeof(GameStatistics),
    //       new GameStatistics() { GameName = "Klondike Solitaire" }); 
    // public GameStatistics KlondikeSolitaireStatistics 
        
    // private NotifyingProperty SpiderSolitaireStatisticsProperty =
    //   new NotifyingProperty("SpiderSolitaireStatistics", typeof(GameStatistics),
    //       new GameStatistics() { GameName = "Spider Solitaire" });
    //GameStatistics SpiderSolitaireStatistics
    // public string DeckFolder
    // new NotifyingProperty("DeckFolder", typeof(string), "Classic");
        
    /// <summary>
    /// The set of available deck folders.
    /// </summary>
    /// <value>The deck folders.</value> 
    public List<string> DeckFolders { get; } = new() { "Classic", "Hearts", "Seasons", "Large Print" };
}