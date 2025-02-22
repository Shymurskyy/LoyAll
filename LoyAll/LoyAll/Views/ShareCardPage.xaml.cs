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

            QrCodeImage.Source = CodeGeneratorHelper.GenerateQrCode(compressedData);
            CompressedDataLabel.Text = compressedData;
        }

        private async void OnCompressedDataTapped(object sender, EventArgs e)
        {
            await Clipboard.SetTextAsync(_compressedData);
            await DisplayAlert("Copied", "Compressed data copied to clipboard!", "OK");
        }
    }
}
