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

namespace SolitaireGames.Casino
{
    /// <summary>
    /// Interaction logic for StatisticsView.xaml
    /// </summary>
    public partial class StatisticsView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticsView"/> class.
        /// </summary>
        public StatisticsView()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// The statistics view model.
        /// </summary>
        private static readonly DependencyProperty GameStatisticsProperty =
          DependencyProperty.Register("GameStatistics", typeof(GameStatistics), typeof(StatisticsView),
          new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the game statistics.
        /// </summary>
        /// <value>The game statistics.</value>
        public GameStatistics GameStatistics
        {
            get { return (GameStatistics)GetValue(GameStatisticsProperty); }
            set { SetValue(GameStatisticsProperty, value); }
        }    
    }
}
