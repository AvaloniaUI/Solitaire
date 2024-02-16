using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace Solitaire.Controls;

public partial class DeckControls : UserControl
{
    public DeckControls()
    {
        InitializeComponent();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BoundsProperty)
        {
            // When there is no "safe area" offset, we want to partially hide TrayContainer.
            // Dispatcher helps with async resizing on some platforms.
            Dispatcher.UIThread.Post(() =>
            {
                var hasOffset = TopLevel.GetTopLevel(this) is { } topLevel
                                && this.TransformToVisual(topLevel) is { } transform
                                && new Rect(default, Bounds.Size).TransformToAABB(transform) is var transformedRect
                                && topLevel.Bounds.Height - transformedRect.Bottom > 1;

                TrayContainer.Classes.Set("downDeck", !hasOffset);
                TrayContainer.Margin = hasOffset ? default : new Thickness(0, 0, 0, -15);
            });
        }
    }
}