using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apex.MVVM;
using Apex.Extensions;
using System.Collections.ObjectModel;

namespace SolitaireGames.SpiderSolitaire
{
    /// <summary>
    /// The different difficulties offered.
    /// </summary>
    public enum Difficulty
    {
        /// <summary>
        /// One suit only.
        /// </summary>
        Easy,

        /// <summary>
        /// Two suits only.
        /// </summary>
        Medium,

        /// <summary>
        /// Four suits.
        /// </summary>
        Hard

    }

    public class SpiderSolitaireViewModel : CardGameViewModel
    {
        public SpiderSolitaireViewModel()
        {
            //  Create the quick access arrays.
            tableaus.Add(tableau1);
            tableaus.Add(tableau2);
            tableaus.Add(tableau3);
            tableaus.Add(tableau4);
            tableaus.Add(tableau5);
            tableaus.Add(tableau6);
            tableaus.Add(tableau7);
            tableaus.Add(tableau8);
            tableaus.Add(tableau9);
            tableaus.Add(tableau10);

            dealCardsCommand = new ViewModelCommand(DoDealCards, true);

            //  If we're in the designer deal a game.
            if (Apex.Design.DesignTime.IsDesignTime)
                DoDealNewGame(null);
        }

        public IList<PlayingCard> GetCardCollection(PlayingCard card)
        {
            if (stock.Contains(card)) return stock;
            foreach (var tableau in tableaus) if (tableau.Contains(card)) return tableau;

            return null;
        }

        protected override void DoDealNewGame(object parameter)
        {
            //  Call the base, which stops the timer, clears
            //  the score etc.
            base.DoDealNewGame(parameter);

            //  Spider solitaire starts with a score of 500.
            Score = 500;

            //  Clear everything.
            stock.Clear();
            foundation.Clear();
            foreach (var tableau in tableaus)
                tableau.Clear();

            //  Create a list of card types.
            List<CardType> eachCardType = new List<CardType>();
            for (int i = 0; i < 8; i++)
            {
                foreach (CardType cardType in Enum.GetValues(typeof(CardType)))
                {
                    eachCardType.Add(cardType);
                }
            }

            //  Create a playing card from each card type.
            //  We just keep on adding cards of suits that depend on the
            //  difficulty setting until we have the required 104.
            List<PlayingCard> playingCards = new List<PlayingCard>();
            foreach (var cardType in eachCardType)
            {
                PlayingCard card = new PlayingCard() { CardType = cardType, IsFaceDown = true };

                switch (Difficulty)
                {
                    case Difficulty.Easy:
                        //  In easy mode, we have hearts only.
                        if (card.Suit != CardSuit.Hearts)
                            continue;
                        break;
                    case Difficulty.Medium:
                        //  In easy mode, we have hearts and spades.
                        if (card.Suit == CardSuit.Diamonds || card.Suit == CardSuit.Clubs)
                            continue;
                        break;
                    case Difficulty.Hard:
                        //  In hard mode we have every card.
                        break;
                }

                //  Add the card.
                playingCards.Add(card);
                
                //  If we've got 104 we're done.
                if (playingCards.Count >= 104)
                    break;
            }
            //  Shuffle the playing cards.
            playingCards.Shuffle();

            //  Now distribute them - do the tableaus first.
            for (int i = 0; i < 54; i++)
            {
                PlayingCard card = playingCards.First();
                playingCards.Remove(card);
                card.IsFaceDown = true;
                tableaus[i % 10].Add(card);
            }
            for (int i = 0; i < 10; i++)
            {
                tableaus[i].Last().IsFaceDown = false;
                tableaus[i].Last().IsPlayable = true;
            }

            //  Finally we add every card that's left over to the stock.
            foreach (var playingCard in playingCards)
            {
                stock.Add(playingCard);
            }
            playingCards.Clear();

            //  And we're done.
            StartTimer();
        }

        private void DoDealCards(object o)
        {
            //  As a sanity check if the stock is empty we cannot deal cards.
            if (stock.Count == 0)
                return;

            //  If any tableau is empty we cannot deal cards.
            foreach (var tableau in tableaus)
                if (tableau.Count == 0)
                    return;

            for (int i = 0; i < 10; i++)
            {
                PlayingCard card = stock.Last();
                stock.Remove(card);
                card.IsFaceDown = false;
                card.IsPlayable = true;
                tableaus[i].Add(card);
            }

            DealCardsCommand.CanExecute = stock.Count > 0;


            //  Check each tableau for sequences - then check for victory.
            CheckEachTableau();
            CheckForVictory();
        }

        public bool MoveCard(ObservableCollection<PlayingCard> from,
            ObservableCollection<PlayingCard> to,
            PlayingCard card, bool checkOnly)
        {
            //  The trivial case is where from and to are the same.
            if (from == to)
                return false;

            //  This is the complicated operation.
            int scoreModifier = 0;

            //  We can move to a tableau only if:
            //  1. It is empty 
            //  2. It is card suit S and we are suit S and Number N-1
            if ((to.Count == 0) ||
                (to.Count > 0 && //to.Last().Suit != card.Suit && 
                (to.Last()).Value == (card.Value + 1)))
            {
                //  Move from tableau to tableau, we always lose a point for this.
                scoreModifier = -1;
            }
            else
                return false;
            
            //  If we were just checking, we're done.
            if (checkOnly)
                return true;

            //  If we've got here we've passed all tests
            //  and move the card and update the score.
            DoMoveCard(from, to, card);
            Score += scoreModifier;
            Moves++;

            //  Check each tableau for sequences - then check for victory.
            CheckEachTableau();
            CheckForVictory();

            return true;
        }

        private void DoMoveCard(ObservableCollection<PlayingCard> from,
            ObservableCollection<PlayingCard> to,
            PlayingCard card)
        {
            //  Indentify the run of cards we're moving.
            List<PlayingCard> run = new List<PlayingCard>();
            for (int i = from.IndexOf(card); i < from.Count; i++)
                run.Add(from[i]);

            //  This function will move the card, as well as setting the 
            //  playable properties of the cards revealed.
            foreach(var runCard in run)
                from.Remove(runCard);
            foreach(var runCard in run)
                to.Add(runCard);

            //  Are there any cards left in the from pile?
            if (from.Count > 0)
            {
                //  Reveal the top card and make it playable.
                PlayingCard topCard = from.Last();

                topCard.IsFaceDown = false;
                topCard.IsPlayable = true;
            }
        }

        /// <summary>
        /// Checks each tableau, making sure that only playable cards
        /// can be played and moving any full sequences to the 
        /// foundation.
        /// </summary>
        private void CheckEachTableau()
        {
            //  Go through each tableau.
            foreach (var tableau in tableaus)
            {
                //  Go through each card in the tableau from top to bottom.   
                bool sequence = true;
                for (int i = tableau.Count - 1, count = 0; i >= 0; i--, count++)
                {
                    //  The first card is always face up and playable.
                    //  Starting with the first card we are hoping to
                    //  find a sequence - only cards in sequence from the top
                    //  down will be playable.
                    if (count == 0)
                    {
                        tableau[i].IsFaceDown = false;
                        tableau[i].IsPlayable = true;
                    }
                    else if (sequence)
                    {
                        //  We are not the top card - do we follow
                        //  in sequence from it - i.e. are we suited and
                        //  with a value one higher?
                        if (tableau[i].Suit == tableau[i + 1].Suit &&
                            tableau[i].Value == tableau[i + 1].Value + 1)
                        {
                            //  We are still in sequence - are we a king?
                            //  If so we've made a full sequence and we
                            //  can remove it.
                            if (tableau[i].Value == 12 && count == 12)
                            {
                                //  Clearing a set gives us an extra 100 points.
                                Score += 100;
                                for (int j = 0; j < 13; j++)
                                {
                                    PlayingCard card = tableau[i];
                                    tableau.Remove(card);
                                    foundation.Add(card);
                                }
                                //  Start again.
                                i = tableau.Count;
                                count = -1;
                                continue;
                            }
                        }
                        else
                        {
                            //  We have broken the sequence.
                            sequence = false;
                        }
                    }

                    //  If the sequence is broken, we're not playable.
                    tableau[i].IsPlayable = sequence;
                }
            }
        }

        public void CheckForVictory()
        {
            //  Every card must be in the foundation for the game to be won.
            if (Foundation.Count < 104)
                return;

            //  We've won.
            IsGameWon = true;

            //  Stop the timer.
            StopTimer();

            //  Fire the won event.
            FireGameWonEvent();
        }

        //  For ease of access we have an array of tableaus.
        List<ObservableCollection<PlayingCard>> tableaus = new List<ObservableCollection<PlayingCard>>();

        //  Ten tableaus in spider solitaire.
        private ObservableCollection<PlayingCard> tableau1 = new ObservableCollection<PlayingCard>();
        private ObservableCollection<PlayingCard> tableau2 = new ObservableCollection<PlayingCard>();
        private ObservableCollection<PlayingCard> tableau3 = new ObservableCollection<PlayingCard>();
        private ObservableCollection<PlayingCard> tableau4 = new ObservableCollection<PlayingCard>();
        private ObservableCollection<PlayingCard> tableau5 = new ObservableCollection<PlayingCard>();
        private ObservableCollection<PlayingCard> tableau6 = new ObservableCollection<PlayingCard>();
        private ObservableCollection<PlayingCard> tableau7 = new ObservableCollection<PlayingCard>();
        private ObservableCollection<PlayingCard> tableau8 = new ObservableCollection<PlayingCard>();
        private ObservableCollection<PlayingCard> tableau9 = new ObservableCollection<PlayingCard>();
        private ObservableCollection<PlayingCard> tableau10 = new ObservableCollection<PlayingCard>();

        //  As in most games there is one stock pile.
        private ObservableCollection<PlayingCard> stock = new ObservableCollection<PlayingCard>();

        //  A single foundation is where we place all completed sequences.
        private ObservableCollection<PlayingCard> foundation = new ObservableCollection<PlayingCard>();

        private ViewModelCommand dealCardsCommand;

        //  Accessors for the various card stacks.
        public ObservableCollection<PlayingCard> Tableau1 { get { return tableau1; } }
        public ObservableCollection<PlayingCard> Tableau2 { get { return tableau2; } }
        public ObservableCollection<PlayingCard> Tableau3 { get { return tableau3; } }
        public ObservableCollection<PlayingCard> Tableau4 { get { return tableau4; } }
        public ObservableCollection<PlayingCard> Tableau5 { get { return tableau5; } }
        public ObservableCollection<PlayingCard> Tableau6 { get { return tableau6; } }
        public ObservableCollection<PlayingCard> Tableau7 { get { return tableau7; } }
        public ObservableCollection<PlayingCard> Tableau8 { get { return tableau8; } }
        public ObservableCollection<PlayingCard> Tableau9 { get { return tableau9; } }
        public ObservableCollection<PlayingCard> Tableau10 { get { return tableau10; } }
        public ObservableCollection<PlayingCard> Stock { get { return stock; } }
        public ObservableCollection<PlayingCard> Foundation { get { return foundation; } }

        private NotifyingProperty DifficultyProperty =
          new NotifyingProperty("Difficulty", typeof(Difficulty), Difficulty.Medium);

        public Difficulty Difficulty
        {
            get { return (Difficulty)GetValue(DifficultyProperty); }
            set { SetValue(DifficultyProperty, value); }
        }

        public ViewModelCommand DealCardsCommand
        {
            get { return dealCardsCommand; }
        }
    }
}
