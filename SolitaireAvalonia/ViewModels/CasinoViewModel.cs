using System;
using System.Collections.Generic; 
using System.IO.IsolatedStorage;
using System.IO;
using System.Xml.Serialization;
using SolitaireAvalonia.Converters;

namespace SolitaireAvalonia.ViewModels
{
    /// <summary>
    /// The casino view model.
    /// </summary>
    public class CasinoViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CasinoViewModel"/> class.
        /// </summary>
        public CasinoViewModel()
        {
            //  Create the commands.
            goToCasinoCommand = new ViewModelCommand(DoGoToCasino, true);
            goToKlondikeSolitaireCommand = new ViewModelCommand(DoGoToKlondikeSolitaire, true);
            goToSpiderSolitaireCommand = new ViewModelCommand(DoGoToSpiderSolitaire, true);
            settingsCommand = new ViewModelCommand(DoSettingsCommand, true);
        }

        /// <summary>
        /// Initialises this instance.
        /// </summary>
        public void Initialise()
        {
            //  We're going to listen out for certain commands in the game
            //  so that we can keep track of scores etc.
            KlondikeSolitaireViewModel.DealNewGameCommand.Executing += new CancelCommandEventHandler(KlondikeDealNewGameCommand_Executing);
            KlondikeSolitaireViewModel.GameWon += new Action(KlondikeSolitaireViewModel_GameWon);
            KlondikeSolitaireViewModel.GoToCasinoCommand.Executed += new CommandEventHandler(GoToCasinoCommand_Executed);
            SpiderSolitaireViewModel.DealNewGameCommand.Executing += new CancelCommandEventHandler(SpiderDealNewGameCommand_Executing);
            SpiderSolitaireViewModel.GameWon += new Action(SpiderSolitaireViewModel_GameWon);
            SpiderSolitaireViewModel.GoToCasinoCommand.Executed +=new CommandEventHandler(GoToCasinoCommand_Executed);

            //  Set the deck we're using for brushes.
            PlayingCardToBrushConverter.SetDeckFolder(DeckFolder);
        }

        /// <summary>
        /// Called when Klondike is won.
        /// </summary>
        void KlondikeSolitaireViewModel_GameWon()
        {
            //  The game was won, update the stats.
            KlondikeSolitaireStatistics.UpdateStatistics(KlondikeSolitaireViewModel);
            Save();
        }

        /// <summary>
        /// Called when Spider is won.
        /// </summary>
        void SpiderSolitaireViewModel_GameWon()
        {
            //  The game was won, update the stats.
            SpiderSolitaireStatistics.UpdateStatistics(KlondikeSolitaireViewModel);
            Save();
        }

        /// <summary>
        /// Handles the Executing event of the DealNewGameCommand control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="Apex.MVVM.CommandEventArgs"/> instance containing the event data.</param>
        void KlondikeDealNewGameCommand_Executing(object sender, CancelCommandEventArgs args)
        {
            //  If we've made any moves, update the stats.
            if (KlondikeSolitaireViewModel.Moves > 0)
                KlondikeSolitaireStatistics.UpdateStatistics(KlondikeSolitaireViewModel);
            Save();
        }

        /// <summary>
        /// Handles the Executed event of the DealNewGameCommand control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="Apex.MVVM.CommandEventArgs"/> instance containing the event data.</param>
        void SpiderDealNewGameCommand_Executing(object sender, CancelCommandEventArgs args)
        {
            //  If we've made any moves, update the stats.
            if (SpiderSolitaireViewModel.Moves > 0)
                SpiderSolitaireStatistics.UpdateStatistics(SpiderSolitaireViewModel);
            Save();
        }

        /// <summary>
        /// Handles the Executed event of the GoToCasinoCommand control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="Apex.MVVM.CommandEventArgs"/> instance containing the event data.</param>
        void GoToCasinoCommand_Executed(object sender, CommandEventArgs args)
        {
            GoToCasinoCommand.DoExecute(null);
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public void Save()
        {
            // Get a new isolated store for this user, domain, and assembly.
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User |
                IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null);

            //  Create data stream.
            using (IsolatedStorageFileStream isoStream =
                new IsolatedStorageFileStream("Casino.xml", FileMode.Create, isoStore))
            {
                XmlSerializer casinoSerializer = new XmlSerializer(typeof(CasinoViewModel));
                casinoSerializer.Serialize(isoStream, this);
            }
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        /// <returns></returns>
        public static CasinoViewModel Load()
        {
            // Get a new isolated store for this user, domain, and assembly.
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User |
                IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null);

            //  Create data stream.
            try
            {
                //  Save the casino.
                using (IsolatedStorageFileStream isoStream =
                    new IsolatedStorageFileStream("Casino.xml", FileMode.Open, isoStore))
                {
                    XmlSerializer casinoSerializer = new XmlSerializer(typeof(CasinoViewModel));
                    return (CasinoViewModel)casinoSerializer.Deserialize(isoStream);
                }
            }
            catch
            {
            }

            return new CasinoViewModel();
        }

        /// <summary>
        /// Goes to the casino.
        /// </summary>
        private void DoGoToCasino()
        {
            KlondikeSolitaireViewModel.StopTimer();
            SpiderSolitaireViewModel.StopTimer();
        }

        /// <summary>
        /// Goes to spider.
        /// </summary>
        private void DoGoToSpiderSolitaire()
        {
            if(SpiderSolitaireViewModel.Moves > 0)
                SpiderSolitaireViewModel.StartTimer();
        }

        /// <summary>
        /// Goes to Klondike.
        /// </summary>
        private void DoGoToKlondikeSolitaire()
        {
            if(KlondikeSolitaireViewModel.Moves > 0)
                KlondikeSolitaireViewModel.StartTimer();
        }

        /// <summary>
        /// The settings command.
        /// </summary>
        private void DoSettingsCommand()
        {
        }

        /// <summary>
        /// The Klondike stats.
        /// </summary>
        private NotifyingProperty KlondikeSolitaireStatisticsProperty =
          new NotifyingProperty("KlondikeSolitaireStatistics", typeof(GameStatistics),
              new GameStatistics() { GameName = "Klondike Solitaire" });

        /// <summary>
        /// Gets or sets the klondike solitaire statistics.
        /// </summary>
        /// <value>The klondike solitaire statistics.</value>
        public GameStatistics KlondikeSolitaireStatistics
        {
            get { return (GameStatistics)GetValue(KlondikeSolitaireStatisticsProperty); }
            set { SetValue(KlondikeSolitaireStatisticsProperty, value); }
        }

        /// <summary>
        /// The spider stats.
        /// </summary>
        private NotifyingProperty SpiderSolitaireStatisticsProperty =
          new NotifyingProperty("SpiderSolitaireStatistics", typeof(GameStatistics),
              new GameStatistics() { GameName = "Spider Solitaire" });

        /// <summary>
        /// Gets or sets the spider solitaire statistics.
        /// </summary>
        /// <value>The spider solitaire statistics.</value>
        public GameStatistics SpiderSolitaireStatistics
        {
            get { return (GameStatistics)GetValue(SpiderSolitaireStatisticsProperty); }
            set { SetValue(SpiderSolitaireStatisticsProperty, value); }
        }

        /// <summary>
        /// The Klondike view model.
        /// </summary>
        private NotifyingProperty KlondikeSolitaireViewModelProperty =
          new NotifyingProperty("KlondikeSolitaireViewModel", typeof(KlondikeSolitaireViewModel), 
              new KlondikeSolitaireViewModel());

        /// <summary>
        /// Gets or sets the klondike solitaire view model.
        /// </summary>
        /// <value>The klondike solitaire view model.</value>
        public KlondikeSolitaireViewModel KlondikeSolitaireViewModel
        {
            get { return (KlondikeSolitaireViewModel)GetValue(KlondikeSolitaireViewModelProperty); }
            set { SetValue(KlondikeSolitaireViewModelProperty, value); }
        }

        /// <summary>
        /// The spider solitaire view model.
        /// </summary>
        private NotifyingProperty SpiderSolitaireViewModelProperty =
          new NotifyingProperty("SpiderSolitaireViewModel", typeof(SpiderSolitaireViewModel), 
              new SpiderSolitaireViewModel());

        /// <summary>
        /// Gets or sets the spider solitaire view model.
        /// </summary>
        /// <value>The spider solitaire view model.</value>
        public SpiderSolitaireViewModel SpiderSolitaireViewModel
        {
            get { return (SpiderSolitaireViewModel)GetValue(SpiderSolitaireViewModelProperty); }
            set { SetValue(SpiderSolitaireViewModelProperty, value); }
        }

        /// <summary>
        /// The selected deck folder.
        /// </summary>
        private NotifyingProperty DeckFolderProperty =
          new NotifyingProperty("DeckFolder", typeof(string), "Classic");

        /// <summary>
        /// Gets or sets the deck folder.
        /// </summary>
        /// <value>The deck folder.</value>
        public string DeckFolder
        {
            get { return (string)GetValue(DeckFolderProperty); }
            set 
            { 
                SetValue(DeckFolderProperty, value);
                PlayingCardToBrushConverter.SetDeckFolder(value);
            }
        }

        /// <summary>
        /// The set of available deck folders.
        /// </summary>
        private List<string> deckFolders = new List<string>() { "Classic", "Hearts", "Seasons", "Large Print" };

        /// <summary>
        /// Gets the deck folders.
        /// </summary>
        /// <value>The deck folders.</value>
        [XmlIgnore]
        public List<string> DeckFolders
        {
            get { return deckFolders; }
        }
        
        /// <summary>
        /// The go to casino command.
        /// </summary>
        private ViewModelCommand goToCasinoCommand;

        /// <summary>
        /// The go to klondike command.
        /// </summary>
        private ViewModelCommand goToKlondikeSolitaireCommand;

        /// <summary>
        /// The spider command.
        /// </summary>
        private ViewModelCommand goToSpiderSolitaireCommand;

        /// <summary>
        /// The settings command.
        /// </summary>
        private ViewModelCommand settingsCommand;

        /// <summary>
        /// Gets the go to casino command.
        /// </summary>
        /// <value>The go to casino command.</value>
        public ViewModelCommand GoToCasinoCommand
        {
            get { return goToCasinoCommand; }
        }

        /// <summary>
        /// Gets the go to klondike solitaire command.
        /// </summary>
        /// <value>The go to klondike solitaire command.</value>
        public ViewModelCommand GoToKlondikeSolitaireCommand
        {
            get { return goToKlondikeSolitaireCommand; }
        }

        /// <summary>
        /// Gets the go to spider solitaire command.
        /// </summary>
        /// <value>The go to spider solitaire command.</value>
        public ViewModelCommand GoToSpiderSolitaireCommand
        {
            get { return goToSpiderSolitaireCommand; }
        }

        /// <summary>
        /// Gets the settings command.
        /// </summary>
        /// <value>The settings command.</value>
        public ViewModelCommand SettingsCommand
        {
            get { return settingsCommand; }
        }
    }
}
