using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Solitaire.Styles;

public sealed partial class Animations : ResourceDictionary
{
    public Animations() => AvaloniaXamlLoader.Load(this);
}
