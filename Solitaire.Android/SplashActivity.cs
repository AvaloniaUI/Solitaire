using Android.App;
using Android.Content;
using AndroidX.AppCompat.App;

namespace Solitaire.Android;

[Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
internal sealed class SplashActivity : AppCompatActivity
{
    protected override void OnResume()
    {
        base.OnResume();

        StartActivity(new Intent(Application.Context, typeof(MainActivity)));
    }
}
