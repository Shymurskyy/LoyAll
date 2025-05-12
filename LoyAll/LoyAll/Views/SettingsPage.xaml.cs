using System.Globalization;
using Java.Util.Functions;
using LoyAll.Helper;
using LoyAll.Services;
using Microsoft.Maui.Controls;

namespace LoyAll
{
    public partial class SettingsPage : ContentPage
    {
        private readonly List<string> _availableLanguages = new() { "Polski", "English", "Deutsch" };
        private readonly MainPage _mainPage;
        public SettingsPage(MainPage mainPage)
        {
            InitializeComponent();

            LanguagePicker.ItemsSource = _availableLanguages;

            LanguagePicker.SelectedItem = LanguageHelper.Instance.GetCurrentLanguage();
            _mainPage = mainPage;
        }

        private void OnTogglePrivacyOptions(object sender, EventArgs e)
        {
            PrivacyOptions.IsVisible = !PrivacyOptions.IsVisible;
        }

        private void OnShowGdprConsentClicked(object sender, EventArgs e)
        {
            GpdrService.InitializeUmpSdk(Platform.CurrentActivity, true);
        }

        private void OnPrivacyPolicyClicked(object sender, EventArgs e)
        {
            Launcher.OpenAsync(new Uri("https://www.termsfeed.com/live/d89f218d-4dbe-4922-a49e-60d2098ade5a"));
        }

        private void OnInviteFriendsClicked(object sender, EventArgs e)
        {
            Share.RequestAsync(new ShareTextRequest
            {
                Text = LanguageHelper.Instance["InviteFriendsText"],
                Title = LanguageHelper.Instance["InviteFriends"]
            });
        }

        private void OnRateUsClicked(object sender, EventArgs e)
        {
            Launcher.OpenAsync(new Uri("market://details?id=com.companyname.loyall"));
        }

        private async void OnDeleteDataClicked(object sender, EventArgs e)
        {
            using (CustomPopup deletePopup = new CustomPopup())
            {
                deletePopup.SetTitle(LanguageHelper.Instance["DeleteData"]);
                deletePopup.SetMessage(LanguageHelper.Instance["DeleteConfirmation"]);

                bool confirm = await deletePopup.ShowConfirmationAsync(this, LanguageHelper.Instance["ConfirmButton"]);
                if (confirm)
                {
                    CardStorageService.DeleteAllCards();
                    await _mainPage.LoadCardsAsync();
                    await Navigation.PopAsync();
                }
            }
        }

        private void OnLanguageSelected(object sender, EventArgs e)
        {
            var selectedLanguage = LanguagePicker.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(selectedLanguage))
            {
                LanguageHelper.Instance.ChangeLanguage(selectedLanguage);
            }
        }

        private async void OnLoyAllTapped(object sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}