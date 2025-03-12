using Android.Content;
using Android.Gms.Ads;
using Android.Gms.Ads.Mediation;
using Google.Ads.Mediation.Admob;
using Plugin.MauiMTAdmob;

public static class AdService
{
    private static bool _isRewardedAdLoaded = false;

    public static void LoadRewardedAd(string adUnitId)
    {
        var activity = Platform.CurrentActivity;
        var preferences = activity.GetSharedPreferences("gdpr_prefs", FileCreationMode.Private);
        bool personalizedAdsAllowed = preferences.GetBoolean("personalized_ads", false);

        var requestBuilder = new AdRequest.Builder();

        if (!personalizedAdsAllowed)
        {
            var extras = new Android.OS.Bundle();
            extras.PutString("npa", "1");
            requestBuilder.AddNetworkExtrasBundle(Java.Lang.Class.FromType(typeof(AdMobAdapter)), extras);
        }

        var request = requestBuilder.Build();

        CrossMauiMTAdmob.Current.OnRewardedLoaded += (s, args) =>
        {
            _isRewardedAdLoaded = true;
        };

        CrossMauiMTAdmob.Current.LoadRewarded(adUnitId);
    }

    public static bool IsRewardedAdLoaded()
    {
        return _isRewardedAdLoaded;
    }

    public static void ShowRewardedAd()
    {
        if (_isRewardedAdLoaded)
        {
            CrossMauiMTAdmob.Current.ShowRewarded();
            _isRewardedAdLoaded = false;
        }
        else
        {
            Console.WriteLine("Reklama nie jest jeszcze załadowana.");
        }
    }
}
