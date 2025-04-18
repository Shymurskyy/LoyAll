using System.Globalization;
using Java.Util.Functions;
using LoyAll.Helper;
using LoyAll.Services;
using Microsoft.Maui.Controls;

namespace LoyAll
{
    public partial class SettingsPage : ContentPage
    {
        private readonly List<string> _availableLanguages = new() { "Polski", "English" };

        public SettingsPage()
        {
            InitializeComponent();

            LanguagePicker.ItemsSource = _availableLanguages;

            LanguagePicker.SelectedItem = LanguageHelper.Instance.GetCurrentLanguage();
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
                Text = "TODO Real Link",
                Title = "TODO"
            });
        }

        private void OnRateUsClicked(object sender, EventArgs e)
        {
            Launcher.OpenAsync(new Uri("market://TODOAPPSTORE"));
        }

        private async void OnDeleteDataClicked(object sender, EventArgs e)
        {
            using (CustomPopup deletePopup = new CustomPopup())
            {
                deletePopup.SetTitle("Usuñ kartê");
                deletePopup.SetMessage($"Czy na pewno chcesz usun¹æ wszystkie dane?");

                bool confirm = await deletePopup.ShowConfirmationAsync(this, "Tak");
                if (confirm)
                    CardStorageService.DeleteAllCards();
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
    }
}