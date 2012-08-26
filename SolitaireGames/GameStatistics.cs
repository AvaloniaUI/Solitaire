using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apex.MVVM;

namespace SolitaireGames
{
    /// <summary>
    /// A set of general statistics for a game.
    /// </summary>
    public class GameStatistics : ViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameStatistics"/> class.
        /// </summary>
        public GameStatistics()
        {
            //  Create the reset command.
            resetCommand = new ViewModelCommand(DoReset, true);
        }

        /// <summary>
        /// Resets the statistics.
        /// </summary>
        private void DoReset()
        {
            GamesPlayed = 0;
            GamesWon = 0; 
            GamesLost = 0; 
            HighestWinningStreak = 0; 
            HighestLosingStreak = 0;
            CurrentStreak = 0; 
            CumulativeScore = 0; 
            HighestScore = 0; 
            AverageScore = 0; 
            CumulativeGameTime = TimeSpan.FromSeconds(0); 
            AverageGameTime = TimeSpan.FromSeconds(0);
        }

        /// <summary>
        /// Updates the statistics based on a won game.
        /// </summary>
        /// <param name="cardGame">The card game.</param>
        public void UpdateStatistics(CardGameViewModel cardGame)
        {
            //  Update the games won or lost.
            GamesPlayed++;
            if (cardGame.IsGameWon)
                GamesWon++;
            else
                GamesLost++;

            //  Update the current streak.
            if (cardGame.IsGameWon)
                CurrentStreak = CurrentStreak < 0 ? 1 : CurrentStreak + 1;
            else
                CurrentStreak = CurrentStreak > 0 ? -1 : CurrentStreak - 1;

            //  Update the highest streaks.
            if (CurrentStreak > HighestWinningStreak)
                HighestWinningStreak = CurrentStreak;
            else if (Math.Abs(CurrentStreak) > HighestLosingStreak)
                HighestLosingStreak = Math.Abs(CurrentStreak);

            //  Update the highest score.
            if (cardGame.Score > HighestScore)
                HighestScore = cardGame.Score;

            //  Update the average score. Only won games
            //  contribute to the running average.
            if (cardGame.IsGameWon)
            {
                CumulativeScore += cardGame.Score;
                AverageScore = CumulativeScore / GamesWon;
            }

            //  Update the average game time.
            CumulativeGameTime += cardGame.ElapsedTime;
            AverageGameTime = TimeSpan.FromTicks(CumulativeGameTime.Ticks / (GamesWon + GamesLost));
        }

        /// <summary>
        /// The game name property.
        /// </summary>
        private NotifyingProperty GameNameProperty =
          new NotifyingProperty("GameName", typeof(string), default(string));

        /// <summary>
        /// Gets or sets the name of the game.
        /// </summary>
        /// <value>The name of the game.</value>
        public string GameName
        {
            get { return (string)GetValue(GameNameProperty); }
            set { SetValue(GameNameProperty, value); }
        }
        
        /// <summary>
        /// The games played property.
        /// </summary>
        private NotifyingProperty GamesPlayedProperty =
          new NotifyingProperty("GamesPlayed", typeof(int), default(int));

        /// <summary>
        /// Gets or sets the games played.
        /// </summary>
        /// <value>The games played.</value>
        public int GamesPlayed
        {
            get { return (int)GetValue(GamesPlayedProperty); }
            set { SetValue(GamesPlayedProperty, value); }
        }

        /// <summary>
        /// The games won property.
        /// </summary>
        private NotifyingProperty GamesWonProperty =
          new NotifyingProperty("GamesWon", typeof(int), default(int));

        /// <summary>
        /// Gets or sets the games won.
        /// </summary>
        /// <value>The games won.</value>
        public int GamesWon
        {
            get { return (int)GetValue(GamesWonProperty); }
            set { SetValue(GamesWonProperty, value); }
        }

        /// <summary>
        /// The games lost property.
        /// </summary>
        private NotifyingProperty GamesLostProperty =
          new NotifyingProperty("GamesLost", typeof(int), default(int));

        /// <summary>
        /// Gets or sets the games lost.
        /// </summary>
        /// <value>The games lost.</value>
        public int GamesLost
        {
            get { return (int)GetValue(GamesLostProperty); }
            set { SetValue(GamesLostProperty, value); }
        }
        
        /// <summary>
        /// The highest winning streak property.
        /// </summary>
        private NotifyingProperty HighestWinningStreakProperty =
          new NotifyingProperty("HighestWinningStreak", typeof(int), default(int));

        /// <summary>
        /// Gets or sets the highest winning streak.
        /// </summary>
        /// <value>The highest winning streak.</value>
        public int HighestWinningStreak
        {
            get { return (int)GetValue(HighestWinningStreakProperty); }
            set { SetValue(HighestWinningStreakProperty, value); }
        }

        /// <summary>
        /// The highest losing streak.
        /// </summary>
        private NotifyingProperty HighestLosingStreakProperty =
          new NotifyingProperty("HighestLosingStreak", typeof(int), default(int));

        /// <summary>
        /// Gets or sets the highest losing streak.
        /// </summary>
        /// <value>The highest losing streak.</value>
        public int HighestLosingStreak
        {
            get { return (int)GetValue(HighestLosingStreakProperty); }
            set { SetValue(HighestLosingStreakProperty, value); }
        }
        
        /// <summary>
        /// The current streak.
        /// </summary>
        private NotifyingProperty CurrentStreakProperty =
          new NotifyingProperty("CurrentStreak", typeof(int), default(int));

        /// <summary>
        /// Gets or sets the current streak.
        /// </summary>
        /// <value>The current streak.</value>
        public int CurrentStreak
        {
            get { return (int)GetValue(CurrentStreakProperty); }
            set { SetValue(CurrentStreakProperty, value); }
        }

        /// <summary>
        /// The cumulative score.
        /// </summary>
        private NotifyingProperty CumulativeScoreProperty =
          new NotifyingProperty("CumulativeScore", typeof(int), default(int));

        /// <summary>
        /// Gets or sets the cumulative score.
        /// </summary>
        /// <value>The cumulative score.</value>
        public int CumulativeScore
        {
            get { return (int)GetValue(CumulativeScoreProperty); }
            set { SetValue(CumulativeScoreProperty, value); }
        }
        
        /// <summary>
        /// The highest score.
        /// </summary>
        private NotifyingProperty HighestScoreProperty =
          new NotifyingProperty("HighestScore", typeof(int), default(int));

        /// <summary>
        /// Gets or sets the highest score.
        /// </summary>
        /// <value>The highest score.</value>
        public int HighestScore
        {
            get { return (int)GetValue(HighestScoreProperty); }
            set { SetValue(HighestScoreProperty, value); }
        }

        /// <summary>
        /// The average score.
        /// </summary>
        private NotifyingProperty AverageScoreProperty =
          new NotifyingProperty("AverageScore", typeof(double), default(double));

        /// <summary>
        /// Gets or sets the average score.
        /// </summary>
        /// <value>The average score.</value>
        public double AverageScore
        {
            get { return (double)GetValue(AverageScoreProperty); }
            set { SetValue(AverageScoreProperty, value); }
        }

        /// <summary>
        /// The cumulative game time.
        /// </summary>
        private NotifyingProperty CumulativeGameTimeProperty =
          new NotifyingProperty("CumulativeGameTime", typeof(double), default(double));

        /// <summary>
        /// Gets or sets the cumulative game time.
        /// </summary>
        /// <value>The cumulative game time.</value>
        public TimeSpan CumulativeGameTime
        {
            get { return TimeSpan.FromSeconds((double)GetValue(CumulativeGameTimeProperty)); }
            set { SetValue(CumulativeGameTimeProperty, value.TotalSeconds); }
        }
        
        /// <summary>
        /// The average game time.
        /// </summary>
        private NotifyingProperty AverageGameTimeProperty =
          new NotifyingProperty("AverageGameTime", typeof(double), default(double));

        /// <summary>
        /// Gets or sets the average game time.
        /// </summary>
        /// <value>The average game time.</value>
        public TimeSpan AverageGameTime
        {
            get { return TimeSpan.FromSeconds((double)GetValue(AverageGameTimeProperty)); }
            set { SetValue(AverageGameTimeProperty, value.TotalSeconds); }
        }

        /// <summary>
        /// The reset command.
        /// </summary>
        private ViewModelCommand resetCommand;

        /// <summary>
        /// Gets the reset command.
        /// </summary>
        /// <value>The reset command.</value>
        public ViewModelCommand ResetCommand
        {
            get { return resetCommand; }
        }
    }
}
