using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SolitaireAvalonia.ViewModels
{
    /// <summary>
    /// Base class for a ViewModel for a card game.
    /// </summary>
    public abstract partial class CardGameViewModel : ViewModelBase
    {
        protected CasinoViewModel CasinoInstance { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CardGameViewModel"/> class.
        /// </summary>
        public CardGameViewModel(CasinoViewModel casinoViewModel)
        {
            CasinoInstance = casinoViewModel;

            NavigateToCasinoCommand =
                new RelayCommand(() => casinoViewModel.CurrentView = casinoViewModel.TitleInstance);
            
            //  Set up the timer.
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += new EventHandler(timer_Tick);

            // //  Create the commands.
            // leftClickCardCommand = new ViewModelCommand(DoLeftClickCard, true);
            // rightClickCardCommand = new ViewModelCommand(DoRightClickCard, true);
            // dealNewGameCommand = new ViewModelCommand(DoDealNewGame, true);
            // goToCasinoCommand = new ViewModelCommand(DoGoToCasino, true);

            DealNewGameCommand = new RelayCommand(DoDealNewGame);
        }

        /// <summary>
        /// The go to casino command.
        /// </summary>
        private void DoGoToCasino()
        {
        }

        /// <summary>
        /// The left click card command.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        protected virtual void DoLeftClickCard(object parameter)
        {
        }

        /// <summary>
        /// The right click card command.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        protected virtual void DoRightClickCard(object parameter)
        {
        }

        public abstract IList<PlayingCardViewModel> GetCardCollection(PlayingCardViewModel card);


        public abstract bool CheckAndMoveCard(IList<PlayingCardViewModel> from,
            IList<PlayingCardViewModel> to,
            PlayingCardViewModel card,
            bool checkOnly = false);
            
        /// <summary>
        /// Deals a new game.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        protected virtual void DoDealNewGame()
        {
            //  Stop the timer and reset the game data.
            StopTimer();
            ElapsedTime = TimeSpan.FromSeconds(0);
            Moves = 0;
            Score = 0;
            IsGameWon = false;
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        public void StartTimer()
        {
            lastTick = DateTime.Now;
            timer.Start();
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void StopTimer()
        {
            timer.Stop();
        }

        /// <summary>
        /// Handles the Tick event of the timer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void timer_Tick(object sender, EventArgs e)
        {
            //  Get the time, update the elapsed time, record the last tick.
            DateTime timeNow = DateTime.Now;
            ElapsedTime += timeNow - lastTick;
            lastTick = timeNow;
        }

        /// <summary>
        /// Fires the game won event.
        /// </summary>
        protected void FireGameWonEvent()
        {
            Action wonEvent = GameWon;
            if (wonEvent != null)
                wonEvent();
        }

        /// <summary>
        /// The timer for recording the time spent in a game.
        /// </summary>
        private DispatcherTimer timer = new  ();

        /// <summary>
        /// The time of the last tick.
        /// </summary>
        private DateTime lastTick;
 
        [ObservableProperty] private int _score;

        [ObservableProperty] private TimeSpan _elapsedTime;

        [ObservableProperty] private int _moves;

        [ObservableProperty] private bool _isGameWon;
 

        /// <summary>
        /// Gets the go to casino command.
        /// </summary>
        /// <value>The go to casino command.</value>
        public ICommand NavigateToCasinoCommand { get; }


        /// <summary>
        /// Gets the deal new game command.
        /// </summary>
        /// <value>The deal new game command.</value>
        public ICommand DealNewGameCommand { get; }

        /// <summary>
        /// Occurs when the game is won.
        /// </summary>
        public event Action GameWon;
        
        public List<PlayingCardViewModel> TemporaryStore { get; } = new ();

    }
}