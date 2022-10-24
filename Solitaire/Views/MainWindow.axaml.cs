using Avalonia;
using Avalonia.Controls;

namespace Solitaire.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

#if DEBUG
        this.AttachDevTools();  
#endif
    }
}