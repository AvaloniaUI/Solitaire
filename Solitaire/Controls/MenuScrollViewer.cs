using Avalonia.Controls;
using Avalonia.Input;

namespace Solitaire.Controls;

public sealed class MenuScrollViewer : ScrollViewer
{
    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        if (e.NavigationMethod is NavigationMethod.Tab or NavigationMethod.Directional &&
            e.Source is Control control && !ReferenceEquals(control, this))
            control.BringIntoView();
    }
}
