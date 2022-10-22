using Avalonia.Controls; 

namespace SolitaireAvalonia.Views
{
    /// <summary>
    /// Interaction logic for CasinoView.xaml
    /// </summary>
    public partial class CasinoView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CasinoView"/> class.
        /// </summary>
        public CasinoView()
        {
            InitializeComponent();
        }
        //
        // /// <summary>
        // /// Handles the Executed event of the GoToKlondikeSolitaireCommand control.
        // /// </summary>
        // /// <param name="sender">The source of the event.</param>
        // /// <param name="args">The <see cref="Apex.MVVM.CommandEventArgs"/> instance containing the event data.</param>
        // void GoToKlondikeSolitaireCommand_Executed(object sender, Apex.MVVM.CommandEventArgs args)
        // {
        //     mainPivot.SelectedPivotItem = mainPivot.PivotItems[0];
        // }
        //
        // /// <summary>
        // /// Handles the Executed event of the GoToSpiderSolitaireCommsand control.
        // /// </summary>
        // /// <param name="sender">The source of the event.</param>
        // /// <param name="args">The <see cref="Apex.MVVM.CommandEventArgs"/> instance containing the event data.</param>
        // void GoToSpiderSolitaireCommsand_Executed(object sender, Apex.MVVM.CommandEventArgs args)
        // {
        //     mainPivot.SelectedPivotItem = mainPivot.PivotItems[2];
        // }
        //
        // /// <summary>
        // /// Handles the Executed event of the GoToCasinoCommand control.
        // /// </summary>
        // /// <param name="sender">The source of the event.</param>
        // /// <param name="args">The <see cref="Apex.MVVM.CommandEventArgs"/> instance containing the event data.</param>
        // void GoToCasinoCommand_Executed(object sender, Apex.MVVM.CommandEventArgs args)
        // {
        //     mainPivot.SelectedPivotItem = mainPivot.PivotItems[1];
        // }
        //
        // /// <summary>
        // /// Handles the Executed event of the SettingsCommand control.
        // /// </summary>
        // /// <param name="sender">The source of the event.</param>
        // /// <param name="args">The <see cref="Apex.MVVM.CommandEventArgs"/> instance containing the event data.</param>
        // private void SettingsCommand_Executed(object sender, Apex.MVVM.CommandEventArgs args)
        // {
        //     mainPivot.SelectedPivotItem = mainPivot.PivotItems[3];
        // }
        //
        // /// <summary>
        // /// CasinoViewModel dependency property.
        // /// </summary>
        // private static readonly DependencyProperty CasinoViewModelProperty =
        //   DependencyProperty.Register("CasinoViewModel", typeof(CasinoViewModel), typeof(CasinoView),
        //   new PropertyMetadata(null, new PropertyChangedCallback(OnCasinoViewModelChanged)));
        //
        // /// <summary>
        // /// Gets or sets the casino view model.
        // /// </summary>
        // /// <value>The casino view model.</value>
        // public CasinoViewModel CasinoViewModel
        // {
        //     get { return (CasinoViewModel)GetValue(CasinoViewModelProperty); }
        //     set { SetValue(CasinoViewModelProperty, value); }
        // }
        //
        // /// <summary>
        // /// Called when [casino view model changed].
        // /// </summary>
        // /// <param name="o">The o.</param>
        // /// <param name="args">The <see cref="Avalonia.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        // private static void OnCasinoViewModelChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
        // {
        //     CasinoView me = o as CasinoView;
        //
        //     //  Listen for events.
        //     me.CasinoViewModel.GoToCasinoCommand.Executed += new Apex.MVVM.CommandEventHandler(me.GoToCasinoCommand_Executed);
        //     me.CasinoViewModel.GoToSpiderSolitaireCommand.Executed += new Apex.MVVM.CommandEventHandler(me.GoToSpiderSolitaireCommsand_Executed);
        //     me.CasinoViewModel.GoToKlondikeSolitaireCommand.Executed += new Apex.MVVM.CommandEventHandler(me.GoToKlondikeSolitaireCommand_Executed);
        //     me.CasinoViewModel.SettingsCommand.Executed += new Apex.MVVM.CommandEventHandler(me.SettingsCommand_Executed);
        // }                
    }
}
