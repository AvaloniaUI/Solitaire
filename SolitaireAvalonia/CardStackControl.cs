using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Documents;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Navigation;
using Avalonia.Shapes;

namespace SolitaireAvalonia
{
    

    public class CardStackControl : ItemsControl
    {
        static CardStackControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CardStackControl), new FrameworkPropertyMetadata(typeof(CardStackControl)));
        }
        
        private static readonly DependencyProperty FaceDownOffsetProperty =
          DependencyProperty.Register("FaceDownOffset", typeof(double), typeof(CardStackControl));

        public double FaceDownOffset
        {
            get { return (double)GetValue(FaceDownOffsetProperty); }
            set { SetValue(FaceDownOffsetProperty, value); }
        }

        private static readonly DependencyProperty FaceUpOffsetProperty =
          DependencyProperty.Register("FaceUpOffset", typeof(double), typeof(CardStackControl));

        public double FaceUpOffset
        {
            get { return (double)GetValue(FaceUpOffsetProperty); }
            set { SetValue(FaceUpOffsetProperty, value); }
        }

        
        private static readonly DependencyProperty OrientationProperty =
          DependencyProperty.Register("Orientation", typeof(Orientation), typeof(CardStackControl),
          new PropertyMetadata(Orientation.Horizontal));

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        private static readonly DependencyProperty OffsetModeProperty =
          DependencyProperty.Register("OffsetMode", typeof(OffsetMode), typeof(CardStackControl),
          new PropertyMetadata(OffsetMode.EveryCard));

        public OffsetMode OffsetMode
        {
            get { return (OffsetMode)GetValue(OffsetModeProperty); }
            set { SetValue(OffsetModeProperty, value); }
        }

        private static readonly DependencyProperty NValueProperty =
          DependencyProperty.Register("NValue", typeof(int), typeof(CardStackControl),
          new PropertyMetadata(1));

        public int NValue
        {
            get { return (int)GetValue(NValueProperty); }
            set { SetValue(NValueProperty, value); }
        }
    }
}
