using System;
using System.Collections.Generic;
using System.Linq; 
using System.Collections.ObjectModel;
using SolitaireAvalonia.Models;

namespace SolitaireAvalonia.ViewModels
{
    /// <summary>
    /// The DrawMode, i.e. how many cards to draw.
    /// </summary>
    public enum DrawMode
    {
        /// <summary>
        /// Draw one card.
        /// </summary>
        DrawOne = 0,

        /// <summary>
        /// Draw three cards.
        /// </summary>
        DrawThree = 1
    }

    /// <summary>
    /// The Klondike Solitaire View Model.
    /// </summary>
    public class KlondikeSolitaireViewModel : CardGameViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KlondikeSolitaireViewModel"/> class.
        /// </summary>
        public KlondikeSolitaireViewModel()
        {
            //  Create the quick access arrays.
            foundations.Add(foundation1);
            foundations.Add(foundation2);
            foundations.Add(foundation3);
            foundations.Add(foundation4);
            tableaus.Add(tableau1);
            tableaus.Add(tableau2);
            tableaus.Add(tableau3);
            tableaus.Add(tableau4);
            tableaus.Add(tableau5);
            tableaus.Add(tableau6);
            tableaus.Add(tableau7);

            //  Create the turn stock command.
            turnStockCommand = new ViewModelCommand(DoTurnStock, true);

            //  If we're in the designer deal a game.
            if (Apex.Design.DesignTime.IsDesignTime)
                DoDealNewGame(null);
        }

        /// <summary>
        /// Gets the card collection for the specified card.
        /// </summary>
        /// <param name="card">The card.</param>
        /// <returns></returns>
        public IList<PlayingCard> GetCardCollection(PlayingCard card)
        {
            if (stock.Contains(card)) return stock;
            if (waste.Contains(card)) return waste;
            foreach (var foundation in foundations) if (foundation.Contains(card)) return foundation;
            foreach (var tableau in tableaus) if (tableau.Contains(card)) return tableau;

            return null;
        }
        
        /// <summary>
        /// Deals a new game.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        protected override void DoDealNewGame(object parameter)
        {
            //  Call the base, which stops the timer, clears
            //  the score etc.
            base.DoDealNewGame(parameter);

            //  Clear everything.
            stock.Clear();
            waste.Clear();
            foreach (var tableau in tableaus)
                tableau.Clear();
            foreach (var foundation in foundations)
                foundation.Clear();

            //  Create a list of card types.
            List<CardType> eachCardType = new List<CardType>();
            foreach (CardType cardType in Enum.GetValues(typeof(CardType)))
                eachCardType.Add(cardType);

            //  Create a playing card from each card type.
            List<PlayingCard> playingCards = new List<PlayingCard>();
            foreach (var cardType in eachCardType)
                playingCards.Add(new PlayingCard() { CardType = cardType, IsFaceDown = true });
        
            //  Shuffle the playing cards.
            playingCards.Shuffle();

            //  Now distribute them - do the tableaus first.
            for (int i = 0; i < 7; i++)
            {
                //  We have i face down cards and 1 face up card.
                for (int j = 0; j < i; j++)
                {
                    PlayingCard faceDownCard = playingCards.First();
                    playingCards.Remove(faceDownCard);
                    faceDownCard.IsFaceDown = true;
                    tableaus[i].Add(faceDownCard);
                }

                //  Add the face up card.
                PlayingCard faceUpCard = playingCards.First();
                playingCards.Remove(faceUpCard);
                faceUpCard.IsFaceDown = false;
                faceUpCard.IsPlayable = true;
                tableaus[i].Add(faceUpCard);
            }

            //  Finally we add every card that's left over to the stock.
            foreach (var playingCard in playingCards)
            {
                playingCard.IsFaceDown = true;
                playingCard.IsPlayable = false;
                stock.Add(playingCard);
            }
            playingCards.Clear();

            //  And we're done.
            StartTimer();
        }

        /// <summary>
        /// Turns cards from the stock into the waste.
        /// </summary>
        private void DoTurnStock()
        {
            //  If the stock is empty, put every card from the waste back into the stock.
            if (stock.Count == 0)
            {
                foreach (var card in waste)
                {
                    card.IsFaceDown = true;
                    card.IsPlayable = false;
                    stock.Insert(0, card);
                }
                waste.Clear();
            }
            else
            {
                //  Everything in the waste so far must now have no offset.
                foreach (var wasteCard in waste)
                    wasteCard.FaceUpOffset = 0;

                //  Work out how many cards to draw.
                int cardsToDraw = 0;
                switch (DrawMode)
                {
                    case DrawMode.DrawOne:
                        cardsToDraw = 1;
                        break;
                    case DrawMode.DrawThree:
                        cardsToDraw = 3;
                        break;
                    default:
                        cardsToDraw = 1;
                        break;
                }

                //  Put up to three cards in the waste.
                for (int i = 0; i < cardsToDraw; i++)
                {
                    if (stock.Count > 0)
                    {
                        PlayingCard card = stock.Last();
                        stock.Remove(card);
                        card.IsFaceDown = false;
                        card.IsPlayable = false;
                        card.FaceUpOffset = 30;
                        waste.Add(card);
                    }
                }
            }

            //  Everything in the waste must be not playable,
            //  apart from the top card.
            foreach (var wasteCard in waste)
                wasteCard.IsPlayable = wasteCard == waste.Last();
        }

        /// <summary>
        /// Tries the move all cards to appropriate foundations.
        /// </summary>
        public void TryMoveAllCardsToAppropriateFoundations()
        {
            //  Go through the top card in each tableau - keeping
            //  track of whether we moved one.
            bool keepTrying = true;
            while (keepTrying)
            {
                bool movedACard = false;
                if (waste.Count > 0)
                    if (TryMoveCardToAppropriateFoundation(waste.Last()))
                        movedACard = true;
                foreach (var tableau in tableaus)
                {
                    if (tableau.Count > 0)
                        if (TryMoveCardToAppropriateFoundation(tableau.Last()))
                            movedACard = true;
                }

                //  We'll keep trying if we moved a card.
                keepTrying = movedACard;
            }
        }

        /// <summary>
        /// Tries the move the card to its appropriate foundation.
        /// </summary>
        /// <param name="card">The card.</param>
        /// <returns>True if card moved.</returns>
        public bool TryMoveCardToAppropriateFoundation(PlayingCard card)
        {
            //  Try the top of the waste first.
            if (waste.LastOrDefault() == card)
                foreach (var foundation in foundations)
                    if (MoveCard(waste, foundation, card, false))
                        return true;

            //  Is the card in a tableau?
            bool inTableau = false;
            int i = 0;
            for (; i < tableaus.Count && inTableau == false; i++)
                inTableau = tableaus[i].Contains(card);

            //  It's if its not in a tablea and it's not the top
            //  of the waste, we cannot move it.
            if (inTableau == false)
                return false;

            //  Try and move to each foundation.
            foreach (var foundation in foundations)
                if (MoveCard(tableaus[i - 1], foundation, card, false))
                    return true;

            //  We couldn't move the card.
            return false;
        }

        /// <summary>
        /// Moves the card.
        /// </summary>
        /// <param name="from">The set we're moving from.</param>
        /// <param name="to">The set we're moving to.</param>
        /// <param name="card">The card we're moving.</param>
        /// <param name="checkOnly">if set to <c>true</c> we only check if we CAN move, but don't actually move.</param>
        /// <returns>True if a card was moved.</returns>
        public bool MoveCard(ObservableCollection<PlayingCard> from,
            ObservableCollection<PlayingCard> to,
            PlayingCard card, bool checkOnly)
        {
            //  The trivial case is where from and to are the same.
            if (from == to)
                return false;

            //  This is the complicated operation.
            int scoreModifier = 0;

            //  Are we moving from the waste?
            if (from == Waste)
            {
                //  Are we moving to a foundation?
                if (foundations.Contains(to))
                {
                    //  We can move to a foundation only if:
                    //  1. It is empty and we are an ace.
                    //  2. It is card SN and we are suit S and Number N+1
                    if ((to.Count == 0 && card.Value == 0) ||
                        (to.Count > 0 && to.Last().Suit == card.Suit && (to.Last()).Value == (card.Value - 1)))
                    {
                        //  Move from waste to foundation.
                        scoreModifier = 10;
                    }
                    else
                        return false;
                }
                //  Are we moving to a tableau?
                else if (tableaus.Contains(to))
                {
                    //  We can move to a tableau only if:
                    //  1. It is empty and we are a king.
                    //  2. It is card CN and we are color !C and Number N-1
                    if ((to.Count == 0 && card.Value == 12) ||
                        (to.Count > 0 && to.Last().Colour != card.Colour && (to.Last()).Value == (card.Value + 1)))
                    {
                        //  Move from waste to tableau.
                        scoreModifier = 5;
                    }
                    else
                        return false;
                }
                //  Any other move from the waste is wrong.
                else
                    return false;
            }
            //  Are we moving from a tableau?
            else if (tableaus.Contains(from))
            {
                //  Are we moving to a foundation?
                if (foundations.Contains(to))
                {
                    //  We can move to a foundation only if:
                    //  1. It is empty and we are an ace.
                    //  2. It is card SN and we are suit S and Number N+1
                    if ((to.Count == 0 && card.Value == 0) ||
                        (to.Count > 0 && to.Last().Suit == card.Suit && (to.Last()).Value == (card.Value - 1)))
                    {
                        //  Move from tableau to foundation.
                        scoreModifier = 10;
                    }
                    else
                        return false;
                }
                //  Are we moving to another tableau?
                else if (tableaus.Contains(to))
                {
                    //  We can move to a tableau only if:
                    //  1. It is empty and we are a king.
                    //  2. It is card CN and we are color !C and Number N-1
                    if ((to.Count == 0 && card.Value == 12) ||
                        (to.Count > 0 && to.Last().Colour != card.Colour && (to.Last()).Value == (card.Value + 1)))
                    {
                        //  Move from tableau to tableau.
                        scoreModifier = 0;
                    }
                    else
                        return false;
                }
                //  Any other move from a tableau is wrong.
                else
                    return false;
            }
            //  Are we moving from a foundation?
            else if (foundations.Contains(from))
            {
                //  Are we moving to a tableau?
                if (tableaus.Contains(to))
                {
                    //  We can move to a tableau only if:
                    //  1. It is empty and we are a king.
                    //  2. It is card CN and we are color !C and Number N-1
                    if ((to.Count == 0 && card.Value == 12) ||
                        (to.Count > 0 && to.Last().Colour != card.Colour && (to.Last()).Value == (card.Value + 1)))
                    {
                        //  Move from foundation to tableau.
                        scoreModifier = -15;
                    }
                    else
                        return false;
                }
                //  Are we moving to another foundation?
                else if (foundations.Contains(to))
                {
                    //  We can move from a foundation to a foundation only 
                    //  if the source foundation has one card (the ace) and the
                    //  destination foundation has no cards).
                    if (from.Count == 1 && to.Count == 0)
                    {
                        //  The move is valid, but has no impact on the score.
                        scoreModifier = 0;
                    }
                    else
                        return false;
                }
                //  Any other move from a foundation is wrong.
                else
                    return false;
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

            //  If we have moved from the waste, we must 
            //  make sure that the top of the waste is playable.
            if (from == Waste && Waste.Count > 0)
                Waste.Last().IsPlayable = true;

            //  Check for victory.
            CheckForVictory();

            return true;
        }

        /// <summary>
        /// Actually moves the card.
        /// </summary>
        /// <param name="from">The stack to move from.</param>
        /// <param name="to">The stack to move to.</param>
        /// <param name="card">The card.</param>
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
        /// Checks for victory.
        /// </summary>
        public void CheckForVictory()
        {
            //  We've won if every foundation is full.
            foreach (var foundation in foundations)
                if (foundation.Count < 13)
                    return;

            //  We've won.
            IsGameWon = true;

            //  Stop the timer.
            StopTimer();

            //  Fire the won event.
            FireGameWonEvent();
        }

        /// <summary>
        /// The right click card command.
        /// </summary>
        /// <param name="parameter">The parameter (should be a playing card).</param>
        protected override void DoRightClickCard(object parameter)
        {
            base.DoRightClickCard(parameter);

            //  Cast the card.
            PlayingCard card = parameter as PlayingCard;
            if (card == null)
                return;

            //  Try and move it to the appropriate foundation.
            TryMoveCardToAppropriateFoundation(card);
        }

        //  For ease of access we have arrays of the foundations and tableaus.
        List<ObservableCollection<PlayingCard>> foundations = new List<ObservableCollection<PlayingCard>>();
        List<ObservableCollection<PlayingCard>> tableaus = new List<ObservableCollection<PlayingCard>>();

        //  Klondike Solitaire has four foundations.
        private ObservableCollection<PlayingCard> foundation1 = new ObservableCollection<PlayingCard>();
        private ObservableCollection<PlayingCard> foundation2 = new ObservableCollection<PlayingCard>();
        private ObservableCollection<PlayingCard> foundation3 = new ObservableCollection<PlayingCard>();
        private ObservableCollection<PlayingCard> foundation4 = new ObservableCollection<PlayingCard>();

        //  It also has seven tableaus.
        private ObservableCollection<PlayingCard> tableau1 = new ObservableCollection<PlayingCard>();
        private ObservableCollection<PlayingCard> tableau2 = new ObservableCollection<PlayingCard>();
        private ObservableCollection<PlayingCard> tableau3 = new ObservableCollection<PlayingCard>();
        private ObservableCollection<PlayingCard> tableau4 = new ObservableCollection<PlayingCard>();
        private ObservableCollection<PlayingCard> tableau5 = new ObservableCollection<PlayingCard>();
        private ObservableCollection<PlayingCard> tableau6 = new ObservableCollection<PlayingCard>();
        private ObservableCollection<PlayingCard> tableau7 = new ObservableCollection<PlayingCard>();

        //  As in most games there is one stock pile.
        private ObservableCollection<PlayingCard> stock = new ObservableCollection<PlayingCard>();

        //  Also, there is the waste pile...
        private ObservableCollection<PlayingCard> waste = new ObservableCollection<PlayingCard>();
        
        /// <summary>
        /// The draw mode property.
        /// </summary>
        private NotifyingProperty DrawModeProperty =
          new NotifyingProperty("DrawMode", typeof(DrawMode), DrawMode.DrawThree);
        
        /// <summary>
        /// Gets or sets the draw mode.
        /// </summary>
        /// <value>The draw mode.</value>
        public DrawMode DrawMode
        {
            get { return (DrawMode)GetValue(DrawModeProperty); }
            set { SetValue(DrawModeProperty, value); }
        }
        
        
        //  Accessors for the various card stacks.
        public ObservableCollection<PlayingCard> Foundation1 { get { return foundation1; } }
        public ObservableCollection<PlayingCard> Foundation2 { get { return foundation2; } }
        public ObservableCollection<PlayingCard> Foundation3 { get { return foundation3; } }
        public ObservableCollection<PlayingCard> Foundation4 { get { return foundation4; } }
        public ObservableCollection<PlayingCard> Tableau1 { get { return tableau1; } }
        public ObservableCollection<PlayingCard> Tableau2 { get { return tableau2; } }
        public ObservableCollection<PlayingCard> Tableau3 { get { return tableau3; } }
        public ObservableCollection<PlayingCard> Tableau4 { get { return tableau4; } }
        public ObservableCollection<PlayingCard> Tableau5 { get { return tableau5; } }
        public ObservableCollection<PlayingCard> Tableau6 { get { return tableau6; } }
        public ObservableCollection<PlayingCard> Tableau7 { get { return tableau7; } }
        public ObservableCollection<PlayingCard> Stock { get { return stock; } }
        public ObservableCollection<PlayingCard> Waste { get { return waste; } }

        /// <summary>
        /// The turn stock command.
        /// </summary>
        private ViewModelCommand turnStockCommand;

        /// <summary>
        /// Gets the turn stock command.
        /// </summary>
        /// <value>The turn stock command.</value>
        public ViewModelCommand TurnStockCommand
        {
            get { return turnStockCommand; }
        }
    }
}
