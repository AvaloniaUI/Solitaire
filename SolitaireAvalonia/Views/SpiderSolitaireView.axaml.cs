using Avalonia.Controls;

namespace SolitaireAvalonia.Views
{
    /// <summary>
    /// Interaction logic for SpiderSolitaireView.xaml
    /// </summary>
    public partial class SpiderSolitaireView : UserControl
    {
        public SpiderSolitaireView()
        {
            InitializeComponent();
        }
        
        //     dragAndDropHost.DragAndDropStart += new DragAndDropDelegate(Instance_DragAndDropStart);
        //     dragAndDropHost.DragAndDropContinue += new DragAndDropDelegate(Instance_DragAndDropContinue);
        //     dragAndDropHost.DragAndDropEnd += new DragAndDropDelegate(Instance_DragAndDropEnd);
        // }
        //
        // void Instance_DragAndDropEnd(object sender, DragAndDropEventArgs args)
        // { 
        //     //  We've put cards temporarily in the drag stack, put them in the 
        //     //  source stack again.                
        //     ItemsControl sourceStack = args.DragSource as ItemsControl;
        //     foreach (var dragCard in draggingCards)
        //         ((ObservableCollection<PlayingCard>)((ItemsControl)args.DragSource).Items).Add(dragCard);
        //
        //     //  If we have a drop target, move the card.
        //     if (args.DropTarget != null)
        //     {
        //         //  Move the card.
        //         ViewModel.MoveCard(
        //             (ObservableCollection<PlayingCard>)((ItemsControl)args.DragSource).Items,
        //             (ObservableCollection<PlayingCard>)((ItemsControl)args.DropTarget).Items,
        //             (PlayingCard)args.DragData, false);
        //     }
        // }
        //
        // void Instance_DragAndDropContinue(object sender, DragAndDropEventArgs args)
        // {
        //     args.Allow = true;
        // }
        //
        // void Instance_DragAndDropStart(object sender, DragAndDropEventArgs args)
        // {
        //     //  The data should be a playing card.
        //     PlayingCard card = args.DragData as PlayingCard;
        //     if (card == null || card.IsPlayable == false)
        //     {
        //         args.Allow = false;
        //         return;
        //     }
        //     args.Allow = true;
        //
        //     //  If the card is draggable, we're going to want to drag the whole
        //     //  stack.
        //     IList<PlayingCard> cards = ViewModel.GetCardCollection(card);
        //     draggingCards = new List<PlayingCard>();
        //     int start = cards.IndexOf(card);
        //     for (int i = start; i < cards.Count; i++)
        //         draggingCards.Add(cards[i]);
        //
        //     //  Clear the drag stack.
        //     dragStack.Items = draggingCards;
        //     dragStack.UpdateLayout();
        //     args.DragAdorner = new Apex.Adorners.VisualAdorner(dragStack);
        //
        //     //  Hide each dragging card.
        //     ItemsControl sourceStack = args.DragSource as ItemsControl;
        //     foreach (var dragCard in draggingCards)
        //         ((ObservableCollection<PlayingCard>)sourceStack.Items).Remove(dragCard);
        // }
        //
        //
        // private static readonly DependencyProperty ViewModelProperty =
        //   DependencyProperty.Register("ViewModel", typeof(SpiderSolitaireViewModel), typeof(SpiderSolitaireView),
        //   new PropertyMetadata(new SpiderSolitaireViewModel(), new PropertyChangedCallback(OnSpiderSolitaireViewModelChanged)));
        //
        // public SpiderSolitaireViewModel ViewModel
        // {
        //     get { return (SpiderSolitaireViewModel)GetValue(ViewModelProperty); }
        //     set { SetValue(ViewModelProperty, value); }
        // }
        //
        // private static void OnSpiderSolitaireViewModelChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
        // {
        //     SpiderSolitaireView me = o as SpiderSolitaireView;
        // }
        //
        // private List<PlayingCard> draggingCards;
        //
        // private void CardStackControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        // {
        //     ViewModel.DealCardsCommand.DoExecute(null);
        // }    
    }
}