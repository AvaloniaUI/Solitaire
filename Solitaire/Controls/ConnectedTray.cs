using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Solitaire.Controls;

// The navigation host can move the frame without changing the content layout.
public partial class ConnectedTray : ContentControl
{
    [GeneratedStyledProperty(DefaultValue = true)]
    public partial bool FrameVisible { get; set; }

    [GeneratedStyledProperty]
    public partial BoxShadows FrameShadow { get; set; }

    [GeneratedStyledProperty]
    public partial bool UseLocalFrameWhenIdle { get; set; }
}
