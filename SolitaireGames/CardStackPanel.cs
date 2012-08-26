using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace SolitaireGames
{
    /// <summary>
    /// The offset mode - how we offset individual cards in a stack.
    /// </summary>
    public enum OffsetMode
    {
        /// <summary>
        /// Offset every card.
        /// </summary>
        EveryCard,

        /// <summary>
        /// Offset every Nth card.
        /// </summary>
        EveryNthCard,

        /// <summary>
        /// Offset only the top N cards.
        /// </summary>
        TopNCards,

        /// <summary>
        /// Offset only the bottom N cards.
        /// </summary>
        BottomNCards,

        /// <summary>
        /// Use the offset values specified in the playing card class (see
        /// PlayingCard.FaceDownOffset and PlayingCard.FaceUpOffset).
        /// </summary>
        UseCardValues
    }

    /// <summary>
    /// A panel for laying out cards.
    /// </summary>
    public class CardStackPanel : StackPanel
    {
        /// <summary>
        /// Infinite size, useful later.
        /// </summary>
        private readonly Size infiniteSpace = new Size(Double.MaxValue, Double.MaxValue);

        /// <summary>
        /// Measures the child elements of a <see cref="T:System.Windows.Controls.StackPanel"/> in anticipation of arranging them during the <see cref="M:System.Windows.Controls.StackPanel.ArrangeOverride(System.Windows.Size)"/> pass.
        /// </summary>
        /// <param name="constraint">An upper limit <see cref="T:System.Windows.Size"/> that should not be exceeded.</param>
        /// <returns>
        /// The <see cref="T:System.Windows.Size"/> that represents the desired size of the element.
        /// </returns>
        protected override Size MeasureOverride(Size constraint)
        {
            //  Keep track of the overall size required.
            Size resultSize = new Size(0, 0);

            //  Get the offsets that each element will need.
            List<Size> offsets = CalculateOffsets();

            //  Calculate the total.
            double totalX = (from o in offsets select o.Width).Sum();
            double totalY = (from o in offsets select o.Height).Sum();

            //  Measure each child (always needed, even if we don't use
            //  the measurement!)
            foreach(UIElement child in Children)
            {
                //  Measure the child against infinite space.
                child.Measure(infiniteSpace);
            }

            //  Add the size of the last element.
            if (LastChild != null)
            {
                //  Add the size.
                totalX += LastChild.DesiredSize.Width;
                totalY += LastChild.DesiredSize.Height;
            }
                        
            return new Size(totalX, totalY);
        }

        /// <summary>
        /// When overridden in a derived class, positions child elements and determines a size for a <see cref="T:System.Windows.FrameworkElement"/> derived class.
        /// </summary>
        /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
        /// <returns>The actual size used.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            double x = 0, y = 0;
            int n = 0;
            int total = Children.Count;

            //  Get the offsets that each element will need.
            List<Size> offsets = CalculateOffsets();
            
            //  If we're going to pass the bounds, deal with it.
            if ((ActualWidth > 0 && finalSize.Width > ActualWidth) || 
                (ActualHeight > 0 && finalSize.Height > ActualHeight))
            {
                //  Work out the amount we have to remove from the offsets.
                double overrunX = finalSize.Width - ActualWidth;
                double overrunY = finalSize.Height - ActualHeight;

                //  Now as a per-offset.
                double dx = overrunX / offsets.Count;
                double dy = overrunY / offsets.Count;

                //  Now nudge each offset.
                for (int i = 0; i < offsets.Count; i++)
                {
                    offsets[i] = new Size(Math.Max(0, offsets[i].Width - dx), Math.Max(0, offsets[i].Height - dy));
                }

                //  Make sure the final size isn't increased past what we can handle.
                finalSize.Width -= overrunX;
                finalSize.Height -= overrunY;
            }

            //  Arrange each child.
            foreach (UIElement child in Children)
            {
                //  Get the card. If we don't have one, skip.
                PlayingCard card = ((FrameworkElement)child).DataContext as PlayingCard;
                if (card == null)
                    continue;

                //  Arrange the child at x,y (the first will be at 0,0)
                child.Arrange(new Rect(x, y, child.DesiredSize.Width, child.DesiredSize.Height));

                //  Update the offset.
                x += offsets[n].Width;
                y += offsets[n].Height;

                //  Increment.
                n++;
            }

            return finalSize;
        }

        /// <summary>
        /// Calculates the offsets.
        /// </summary>
        /// <returns></returns>
        private List<Size> CalculateOffsets()
        {
            //  Calculate the offsets on a card by card basis.
            List<Size> offsets = new List<Size>();

            int n = 0;
            int total = Children.Count;

            //  Go through each card.
            foreach (UIElement child in Children)
            {
                //  Get the card. If we don't have one, skip.
                PlayingCard card = ((FrameworkElement)child).DataContext as PlayingCard;
                if (card == null)
                    continue;

                //  The amount we'll offset by.
                double faceDownOffset = 0;
                double faceUpOffset = 0;

                //  We are now going to offset only if the offset mode is appropriate.
                switch (OffsetMode)
                {
                    case OffsetMode.EveryCard:
                        //  Offset every card.
                        faceDownOffset = FaceDownOffset;
                        faceUpOffset = FaceUpOffset;
                        break;
                    case OffsetMode.EveryNthCard:
                        //  Offset only if n Mod N is zero.
                        if (((n + 1) % NValue) == 0)
                        {
                            faceDownOffset = FaceDownOffset;
                            faceUpOffset = FaceUpOffset;
                        }
                        break;
                    case OffsetMode.TopNCards:
                        //  Offset only if (Total - N) <= n < Total
                        if ((total - NValue) <= n && n < total)
                        {
                            faceDownOffset = FaceDownOffset;
                            faceUpOffset = FaceUpOffset;
                        }
                        break;
                    case OffsetMode.BottomNCards:
                        //  Offset only if 0 < n < N
                        if (n < NValue)
                        {
                            faceDownOffset = FaceDownOffset;
                            faceUpOffset = FaceUpOffset;
                        }
                        break;
                    case SolitaireGames.OffsetMode.UseCardValues:
                        //  Offset each time by the amount specified in the card object.
                        faceDownOffset = card.FaceDownOffset;
                        faceUpOffset = card.FaceUpOffset;
                        break;
                    default:
                        break;
                }

                n++;

                //  Create the offset as a size.
                Size offset = new Size(0, 0);
                
                //  Offset.
                switch (Orientation)
                {
                    case Orientation.Horizontal:
                        offset.Width = card.IsFaceDown ? faceDownOffset : faceUpOffset;
                        break;
                    case Orientation.Vertical:
                        offset.Height = card.IsFaceDown ? faceDownOffset : faceUpOffset;
                        break;
                    default:
                        break;
                }

                //  Add to the list.
                offsets.Add(offset);
            }

            return offsets;
        }

        /// <summary>
        /// Gets the last child.
        /// </summary>
        /// <value>The last child.</value>
        private UIElement LastChild
        {
            get { return Children.Count > 0 ? Children[Children.Count - 1] : null; }
        }

        /// <summary>
        /// Face down offset.
        /// </summary>
        private static readonly DependencyProperty FaceDownOffsetProperty =
          DependencyProperty.Register("FaceDownOffset", typeof(double), typeof(CardStackPanel),
          new FrameworkPropertyMetadata(5.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// Gets or sets the face down offset.
        /// </summary>
        /// <value>The face down offset.</value>
        public double FaceDownOffset
        {
            get { return (double)GetValue(FaceDownOffsetProperty); }
            set { SetValue(FaceDownOffsetProperty, value); }
        }

        /// <summary>
        /// Face up offset.
        /// </summary>
        private static readonly DependencyProperty FaceUpOffsetProperty =
          DependencyProperty.Register("FaceUpOffset", typeof(double), typeof(CardStackPanel),
          new FrameworkPropertyMetadata(5.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// Gets or sets the face up offset.
        /// </summary>
        /// <value>The face up offset.</value>
        public double FaceUpOffset
        {
            get { return (double)GetValue(FaceUpOffsetProperty); }
            set { SetValue(FaceUpOffsetProperty, value); }
        }

        /// <summary>
        /// The offset mode.
        /// </summary>
        private static readonly DependencyProperty OffsetModeProperty =
          DependencyProperty.Register("OffsetMode", typeof(OffsetMode), typeof(CardStackPanel),
          new PropertyMetadata(OffsetMode.EveryCard));

        /// <summary>
        /// Gets or sets the offset mode.
        /// </summary>
        /// <value>The offset mode.</value>
        public OffsetMode OffsetMode
        {
            get { return (OffsetMode)GetValue(OffsetModeProperty); }
            set { SetValue(OffsetModeProperty, value); }
        }

        /// <summary>
        /// The NValue, used for some modes.
        /// </summary>
        private static readonly DependencyProperty NValueProperty =
          DependencyProperty.Register("NValue", typeof(int), typeof(CardStackPanel),
          new PropertyMetadata(1));

        /// <summary>
        /// Gets or sets the N value.
        /// </summary>
        /// <value>The N value.</value>
        public int NValue
        {
            get { return (int)GetValue(NValueProperty); }
            set { SetValue(NValueProperty, value); }
        }
    }
}
