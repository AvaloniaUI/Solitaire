using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace Solitaire.Platform.Browser;

[SupportedOSPlatform("browser")]
public static partial class LoaderInterop
{
    [JSImport("globalThis.solitaireLoader.begin")]
    public static partial void Begin(double relativeX, double relativeY, double relativeWidth, double relativeHeight);

    [JSImport("globalThis.solitaireLoader.ready")]
    public static partial void Ready();
}
