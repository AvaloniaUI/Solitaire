using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Apex.DragAndDrop;

namespace SolitaireGames.KlondikeSolitaire
{
    /// <summary>
    /// Interaction logic for KlondikeSolitaireView.xaml
    /// </summary>
    public partial class KlondikeSolitaireView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KlondikeSolitaireView"/> class.
        /// </summary>
        public KlondikeSolitaireView()
        {
            InitializeComponent();

            //  Wire up the drag and drop host.
            dragAndDropHost.DragAndDropStart += new DragAndDropDelegate(Instance_DragAndDropStart);
            dragAndDropHost.DragAndDropContinue += new DragAndDropDelegate(Instance_DragAndDropContinue);
            dragAndDropHost.DragAndDropEnd += new DragAndDropDelegate(Instance_DragAndDropEnd);
        }

        void Instance_DragAndDropEnd(object sender, DragAndDropEventArgs args)
        {
            //  We've put cards temporarily in the drag stack, put them in the 
            //  source stack again.                
            ItemsControl sourceStack = args.DragSource as ItemsControl;
            foreach (var dragCard in draggingCards)
                ((ObservableCollection<PlayingCard>)((ItemsControl)args.DragSource).ItemsSource).Add(dragCard);

            //  If we have a drop target, move the card.
            if (args.DropTarget != null)
            {
                //  Move the card.
                ViewModel.MoveCard(
                    (ObservableCollection<PlayingCard>)((ItemsControl)args.DragSource).ItemsSource,
                    (ObservableCollection<PlayingCard>)((ItemsControl)args.DropTarget).ItemsSource,
                    (PlayingCard)args.DragData, false);
            }
        }

        void Instance_DragAndDropContinue(object sender, DragAndDropEventArgs args)
        {
            args.Allow = true;
        }

        void Instance_DragAndDropStart(object sender, DragAndDropEventArgs args)
        {
            //  The data should be a playing card.
            PlayingCard card = args.DragData as PlayingCard;
            if (card == null || card.IsPlayable == false)
            {
                args.Allow = false;
                return;
            }
            args.Allow = true;

            //  If the card is draggable, we're going to want to drag the whole
            //  stack.
            IList<PlayingCard> cards = ViewModel.GetCardCollection(card);
            draggingCards = new List<PlayingCard>();
            int start = cards.IndexOf(card);
            for (int i = start; i < cards.Count; i++)
                draggingCards.Add(cards[i]);

            //  Clear the drag stack.
            dragStack.ItemsSource = draggingCards;
            dragStack.UpdateLayout();
            args.DragAdorner = new Apex.Adorners.VisualAdorner(dragStack);

            //  Hide each dragging card.
            ItemsControl sourceStack = args.DragSource as ItemsControl;
            foreach (var dragCard in draggingCards)
                ((ObservableCollection<PlayingCard>)sourceStack.ItemsSource).Remove(dragCard);
        }


        /// <summary>
        /// The ViewModel dependency property.
        /// </summary>
        private static readonly DependencyProperty ViewModelProperty =
          DependencyProperty.Register("ViewModel", typeof(KlondikeSolitaireViewModel), typeof(KlondikeSolitaireView),
          new PropertyMetadata(new KlondikeSolitaireViewModel()));

        /// <summary>
        /// Gets or sets the view model.
        /// </summary>
        /// <value>The view model.</value>
        public KlondikeSolitaireViewModel ViewModel
        {
            get { return (KlondikeSolitaireViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        /// <summary>
        /// Handles the MouseRightButtonDown event of the dragAndDropHost control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void dragAndDropHost_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.TryMoveAllCardsToAppropriateFoundations();
        }

        /// <summary>
        /// Temporary storage for cards being dragged.
        /// </summary>
        private List<PlayingCard> draggingCards;

        /// <summary>
        /// Handles the MouseLeftButtonUp event of the CardStackControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void CardStackControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ViewModel.TurnStockCommand.DoExecute(null);
        }
    }
}
