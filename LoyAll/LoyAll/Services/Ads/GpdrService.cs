using Android.App;
using Android.Content;
using Android.Gms.Ads;
using Android.Runtime;
using Plugin.MauiMTAdmob.Extra;
using QRCoder.Extensions;
using Xamarin.Google.UserMesssagingPlatform;
using static AndroidX.ConstraintLayout.Core.Motion.Utils.HyperSpline;
using static Xamarin.Google.UserMesssagingPlatform.UserMessagingPlatform;
public static class GpdrService
{
    private const string PREFS_NAME = "gdpr_prefs";
    private const string KEY_CONSENT_GIVEN = "consent_given";
    private const string KEY_PERSONALIZED_ADS = "personalized_ads";

    public static void InitializeUmpSdk(Activity activity, bool forceShowConsent = false)
    {
        var preferences = activity.GetSharedPreferences(PREFS_NAME, FileCreationMode.Private);
        bool consentGiven = preferences.GetBoolean(KEY_CONSENT_GIVEN, false);

        if (consentGiven && !forceShowConsent) return;

        var consentRequest = new ConsentRequestParameters.Builder()
            .SetTagForUnderAgeOfConsent(false)
            .Build();

        var consentInformation = UserMessagingPlatform.GetConsentInformation(activity);
        consentInformation.RequestConsentInfoUpdate(
            activity,
            consentRequest,
            new ConsentInfoUpdateListener(activity, preferences),
            new ConsentInfoUpdateFailureListener()
        );
    }

    public static bool IsPersonalizedAdsAllowed(Activity activity)
    {
        var preferences = activity.GetSharedPreferences(PREFS_NAME, FileCreationMode.Private);
        return preferences.GetBoolean(KEY_PERSONALIZED_ADS, false);
    }
}

public class ConsentInfoUpdateListener : Java.Lang.Object, IConsentInformationOnConsentInfoUpdateSuccessListener
{
    private readonly Activity _activity;
    private readonly ISharedPreferences _preferences;

    public ConsentInfoUpdateListener(Activity activity, ISharedPreferences preferences)
    {
        _activity = activity;
        _preferences = preferences;
    }

    public void OnConsentInfoUpdateSuccess()
    {
        var context = Android.App.Application.Context;
        var consentInformation = UserMessagingPlatform.GetConsentInformation(context);

        if (consentInformation.IsConsentFormAvailable)
        {
            UserMessagingPlatform.LoadConsentForm(
                context,
                new ConsentFormLoadListener(_activity, _preferences),
                new ConsentFormLoadFailureListener()
            );
        }
    }
}

public class ConsentInfoUpdateFailureListener : Java.Lang.Object, IConsentInformationOnConsentInfoUpdateFailureListener
{
    public void OnConsentInfoUpdateFailure(FormError error)
    {
        Console.WriteLine($"Błąd aktualizacji zgody: {error.Message}");
    }
}

public class ConsentFormLoadListener : Java.Lang.Object, IOnConsentFormLoadSuccessListener
{
    private readonly Activity _activity;
    private readonly ISharedPreferences _preferences;

    public ConsentFormLoadListener(Activity activity, ISharedPreferences preferences)
    {
        _activity = activity;
        _preferences = preferences;
    }

    public void OnConsentFormLoadSuccess(IConsentForm consentForm)
    {
        consentForm.Show(_activity, new ConsentFormDismissedListener(_preferences));
    }
}

public class ConsentFormLoadFailureListener : Java.Lang.Object, IOnConsentFormLoadFailureListener
{
    public void OnConsentFormLoadFailure(FormError error)
    {
        Console.WriteLine($"Błąd ładowania formularza: {error.Message}");
    }
}

public class ConsentFormDismissedListener : Java.Lang.Object, IConsentFormOnConsentFormDismissedListener
{
    private readonly ISharedPreferences _preferences;

    public ConsentFormDismissedListener(ISharedPreferences preferences)
    {
        _preferences = preferences;
    }

    public void OnConsentFormDismissed(FormError error)
    {
        var context = Android.App.Application.Context;
        var consentInformation = UserMessagingPlatform.GetConsentInformation(context);

        var editor = _preferences.Edit();
        editor.PutBoolean("consent_given", true);

        bool personalizedAds = consentInformation.CanRequestAds() &&
                               consentInformation.ConsentStatus == (int)ConsentStatus.Obtained;

        editor.PutBoolean("personalized_ads", personalizedAds);
        editor.Apply();

        Console.WriteLine($"Zgoda na reklamy personalizowane: {personalizedAds}");
    }
}
