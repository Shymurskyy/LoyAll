using Android.App;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.OS;
using Plugin.MauiMTAdmob;

namespace LoyAll
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            CrossMauiMTAdmob.Current.Init(this, "xxx");
        }
    }
}
