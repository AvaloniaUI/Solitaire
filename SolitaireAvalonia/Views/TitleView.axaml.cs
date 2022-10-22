using Avalonia.Controls;
using SolitaireAvalonia.ViewModels;

namespace SolitaireAvalonia.Views
{
    /// <summary>
    /// Interaction logic for StatisticsView.xaml
    /// </summary>
    public partial class TitleView : UserControl
    {
        public TitleView()
        {
            InitializeComponent();
        }
        //
        // /// <summary>
        // /// The statistics view model.
        // /// </summary>
        // private static readonly DependencyProperty GameStatisticsProperty =
        //   DependencyProperty.Register("GameStatistics", typeof(GameStatistics), typeof(StatisticsView),
        //   new PropertyMetadata(null));
        //
        // /// <summary>
        // /// Gets or sets the game statistics.
        // /// </summary>
        // /// <value>The game statistics.</value>
        // public GameStatistics GameStatistics
        // {
        //     get { return (GameStatistics)GetValue(GameStatisticsProperty); }
        //     set { SetValue(GameStatisticsProperty, value); }
        // }    
    }
}