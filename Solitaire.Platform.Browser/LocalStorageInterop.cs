using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace Solitaire.Platform.Browser;

[SupportedOSPlatform("browser")]
public static partial class LocalStorageInterop
{
    [JSImport("globalThis.localStorage.setItem")]
    public static partial void SetItem(string key, string value);

    [JSImport("globalThis.localStorage.getItem")]
    public static partial string? GetItem(string key);
}
