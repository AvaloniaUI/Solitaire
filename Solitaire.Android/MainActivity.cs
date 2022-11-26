using Android.App;
using Android.Content.PM;
using Android.OS;
using Avalonia;
using Avalonia.Android;

namespace Solitaire.Android;

[Activity(Label = "Solitaire.Android", Theme = "@style/MyTheme.NoActionBar", Icon = "@drawable/icon", LaunchMode = LaunchMode.SingleInstance, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
public class MainActivity : AvaloniaMainActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        RequestedOrientation = ScreenOrientation.Landscape;
        base.OnCreate(savedInstanceState);
    }
}