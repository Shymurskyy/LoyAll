using System;
using Microsoft.Maui.Controls;

namespace LoyAll
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void OnTogglePrivacyOptions(object sender, EventArgs e)
        {
            PrivacyOptions.IsVisible = !PrivacyOptions.IsVisible;
        }

        private void OnShowGdprConsentClicked(object sender, EventArgs e)
        {
            GpdrService.InitializeUmpSdk(Platform.CurrentActivity, true);
        }
    }
}
