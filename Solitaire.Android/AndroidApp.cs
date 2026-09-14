using Android.App;
using Android.Runtime;
using Avalonia.Android;

namespace Solitaire.Android;

[Application]
internal sealed class AndroidApp : AvaloniaAndroidApplication<App>
{
    public AndroidApp(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
    }
}
