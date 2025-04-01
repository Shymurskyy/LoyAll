using Microsoft.Maui.Controls;
using LoyAll.Helper;
using System;

namespace LoyAll.Views
{
    public partial class ShareCardPage : ContentPage
    {
        private string _compressedData;

        public ShareCardPage(string compressedData)
        {
            InitializeComponent();
            _compressedData = compressedData;

            QrCodeImage.Source = BarcodeHelper.GenerateQrCode(compressedData);
            CompressedDataLabel.Text = compressedData;
        }
        private async void OnHelpClicked(object sender, EventArgs e)
        {
            await DisplayAlert("","Tutaj mo¿esz udostêpniæ swoje karty lojalnoœciowe. Wygeneruj kod QR lub skopiuj link a nastêpnie zaimportuj na innym urz¹dzeniu aby dzieliæ siê tymi samymi kartami!", "OK");
        }
        private async void OnCompressedDataTapped(object sender, EventArgs e)
        {
            await Clipboard.SetTextAsync(_compressedData);
            await ShowToast("Skopiowano do schowka");
        }

        private async Task ShowToast(string message)
        {
            if (ToastMessage.Content is Label label)
            {
                label.Text = message;
            }
            ToastMessage.Opacity = 0;
            ToastMessage.TranslationY = 50;

            await Task.WhenAll(
                ToastMessage.FadeTo(1, 800, Easing.SinIn),
                ToastMessage.TranslateTo(0, 0, 800, Easing.SinIn)
            );

            await Task.Delay(4000);

            await Task.WhenAll(
                ToastMessage.FadeTo(0, 600, Easing.SinOut),
                ToastMessage.TranslateTo(0, -20, 600, Easing.SinOut)
            );

            ToastMessage.TranslationY = 50;
        }
    }
}
