using Avalonia.Controls;
using Avalonia.Automation;

namespace Solitaire.Views.Pages;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        AutomationProperties.SetName(DirectionDial, "Light direction");
        AutomationProperties.SetName(HeightSlider, "Light height");
    }
}
