using System;
using Avalonia.Threading;

namespace SolitaireAvalonia.ViewModels
{
    /// <summary>
    /// Base class for a ViewModel for a card game.
    /// </summary>
    public class CardGameViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardGameViewModel"/> class.
        /// </summary>
        public CardGameViewModel()
        {
            //  Set up the timer.
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += new EventHandler(timer_Tick);

            //  Create the commands.
            leftClickCardCommand = new ViewModelCommand(DoLeftClickCard, true);
            rightClickCardCommand = new ViewModelCommand(DoRightClickCard, true);
            dealNewGameCommand = new ViewModelCommand(DoDealNewGame, true);
            goToCasinoCommand = new ViewModelCommand(DoGoToCasino, true);
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

        /// <summary>
        /// Deals a new game.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        protected virtual void DoDealNewGame(object parameter)
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
        private DispatcherTimer timer = new DispatcherTimer();

        /// <summary>
        /// The time of the last tick.
        /// </summary>
        private DateTime lastTick;

        /// <summary>
        /// The score notifying property.
        /// </summary>
        private NotifyingProperty scoreProperty = new NotifyingProperty("Score", typeof(int), 0);

        /// <summary>
        /// Gets or sets the score.
        /// </summary>
        /// <value>The score.</value>
        public int Score
        {
            get { return (int)GetValue(scoreProperty); }
            set { SetValue(scoreProperty, value); }
        }
        
        /// <summary>
        /// The elapsed time property.
        /// </summary>
        private readonly NotifyingProperty elapsedTimeProperty =
            new NotifyingProperty("ElapsedTime", typeof(double), default(double));

        /// <summary>
        /// Gets or sets the elapsed time.
        /// </summary>
        /// <value>The elapsed time.</value>
        public TimeSpan ElapsedTime
        {
            get { return TimeSpan.FromSeconds((double)GetValue(elapsedTimeProperty)); }
            set { SetValue(elapsedTimeProperty, value.TotalSeconds); }
        }

        /// <summary>
        /// The moves notifying property.
        /// </summary>
        private readonly NotifyingProperty movesProperty =
            new NotifyingProperty("Moves", typeof(int), 0);

        /// <summary>
        /// Gets or sets the moves.
        /// </summary>
        /// <value>The moves.</value>
        public int Moves
        {
            get { return (int)GetValue(movesProperty); }
            set { SetValue(movesProperty, value); }
        }

        /// <summary>
        /// The victory flag.
        /// </summary>
        private NotifyingProperty isGameWonProperty = new NotifyingProperty("IsGameWon",
            typeof(bool), false);

        /// <summary>
        /// Gets or sets a value indicating whether this instance is game won.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is game won; otherwise, <c>false</c>.
        /// </value>
        public bool IsGameWon
        {
            get { return (bool)GetValue(isGameWonProperty); }
            set { SetValue(isGameWonProperty, value); }
        }

        /// <summary>
        /// The left click card command.
        /// </summary>
        private ViewModelCommand leftClickCardCommand;

        /// <summary>
        /// Gets the left click card command.
        /// </summary>
        /// <value>The left click card command.</value>
        public ViewModelCommand LeftClickCardCommand
        {
            get { return leftClickCardCommand; }
        }

        /// <summary>
        /// The right click card command.
        /// </summary>
        private ViewModelCommand rightClickCardCommand;

        /// <summary>
        /// Gets the right click card command.
        /// </summary>
        /// <value>The right click card command.</value>
        public ViewModelCommand RightClickCardCommand
        {
            get { return rightClickCardCommand; }
        }

        /// <summary>
        /// The command to go to the casino.
        /// </summary>
        private ViewModelCommand goToCasinoCommand;

        /// <summary>
        /// Gets the go to casino command.
        /// </summary>
        /// <value>The go to casino command.</value>
        public ViewModelCommand GoToCasinoCommand
        {
            get { return goToCasinoCommand; }
        }

        /// <summary>
        /// The command to deal a new game.
        /// </summary>
        private ViewModelCommand dealNewGameCommand;

        /// <summary>
        /// Gets the deal new game command.
        /// </summary>
        /// <value>The deal new game command.</value>
        public ViewModelCommand DealNewGameCommand
        {
            get { return dealNewGameCommand; }
        }

        /// <summary>
        /// Occurs when the game is won.
        /// </summary>
        public event Action GameWon;
    }
}
