using LoyAll.Helper;
using LoyAll.Model;
using LoyAll.Services;
using LZStringCSharp;
using Newtonsoft.Json;
using SkiaSharp;
using System.IO;
using System.Text.Json.Serialization;
using ZXing;
using ZXing.Net.Maui;
using ZXing.SkiaSharp;

namespace LoyAll.Views
{
    public partial class AddCardPage : ContentPage
    {
        private string selectedImagePath;
        private MainPage mainPage;

        public AddCardPage(MainPage mainPage)
        {
            InitializeComponent();
            this.mainPage = mainPage;
        }

        private async void OnPickImageClicked(object sender, EventArgs e)
        {
            FileResult? result = await MediaPicker.PickPhotoAsync();
            if (result != null)
            {
                Stream stream = await result.OpenReadAsync();
                CardImage.Source = ImageSource.FromStream(() => stream);
                selectedImagePath = result.FullPath;

                string barcodeValue = await BarcodeHelper.DecodeBarcodeFromImage(stream);
                if (!string.IsNullOrEmpty(barcodeValue) )
                {
                    if (barcodeValue.StartsWith("Q:#") || barcodeValue.StartsWith("B:#"))
                        BarcodeEntry.Text = barcodeValue; 
                    else
                        BarcodeEntry.Text = "Nieobs³ugiwany kod";
                }
                else
                {
                    await DisplayAlert("B³¹d", "Nie znaleziono kodu QR/kreskowego na obrazie.", "OK");
                }
            }
        }

        private async void OnSaveCardClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(StoreNameEntry.Text) || CardImage.Source == null)
            {
                await DisplayAlert("B³¹d", "Podaj nazwê i wybierz obrazek!", "OK");
                return;
            }

            Card card = new Card
            {
                StoreName = StoreNameEntry.Text,
                CardValue = BarcodeEntry.Text
            };

            CardStorageService.SaveCard(card);
            mainPage.AddCard(card);
            await Navigation.PopAsync();
        }

        private void OnBarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
        {
            BarcodeResult? firstBarcode = e.Results.FirstOrDefault();
            if (firstBarcode != null)
            {
                string barcodeValue = firstBarcode.Value;
                BarcodeEntry.Text = barcodeValue;
            }
        }
        private async void OnImportSharedCardClicked(object sender, EventArgs e)
        {
            try
            {
                var action = await DisplayActionSheet("Wybierz sposób importu", "Anuluj", null, "Import z obrazu", "Import z tekstu");

                if (action == "Import z obrazu")
                {
                    FileResult? result = await MediaPicker.PickPhotoAsync();
                    if (result != null)
                    {
                        Stream stream = await result.OpenReadAsync();
                        string decodedValue = LZString.DecompressFromEncodedURIComponent(await BarcodeHelper.DecodeBarcodeFromImage(stream, true));
                        var tempCards = JsonConvert.DeserializeObject<List<dynamic>>(decodedValue);
                        var importedCards = tempCards.Select(x => new Card { StoreName = x.n, CardValue = x.k }).ToList();

                        if (importedCards != null)
                        {
                            foreach (var item in importedCards)
                            {
                                CardStorageService.SaveCard(item);
                                mainPage.AddCard(item);
                            }
                        }
                    }
                }
                else if (action == "Import z tekstu")
                {
                    string inputText = await DisplayPromptAsync("Import z tekstu", "Wklej zakodowan¹ wiadomoœæ:");
                    if (!string.IsNullOrWhiteSpace(inputText))
                    {
                        try
                        {
                            string decodedValue = LZString.DecompressFromEncodedURIComponent(inputText);
                            var tempCards = JsonConvert.DeserializeObject<List<dynamic>>(decodedValue);
                            var importedCards = tempCards.Select(x => new Card { StoreName = x.n, CardValue = x.k }).ToList();

                            if (importedCards != null)
                            {
                                foreach (var item in importedCards)
                                {
                                    CardStorageService.SaveCard(item);
                                    mainPage.AddCard(item);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            await DisplayAlert("B³¹d", $"Nie uda³o siê zdekodowaæ wiadomoœci: {ex.Message}", "OK");
                        }
                    }
                }

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("B³¹d", $"Nie uda³o siê zdekodowaæ kodu: {ex.Message}", "OK");
            }
        }


        private async void OnScanBarcodeClicked(object sender, EventArgs e)
        {
            try
            {
                var photo = await MediaPicker.CapturePhotoAsync();

                if (photo != null)
                {
                    Stream photoStream = await photo.OpenReadAsync();

                    string barcodeValue = await BarcodeHelper.DecodeBarcodeFromImage(photoStream);

                    if (!string.IsNullOrEmpty(barcodeValue))
                    {
                        if (barcodeValue.StartsWith("Q:#") || barcodeValue.StartsWith("B:#"))
                        {
                            BarcodeEntry.Text = barcodeValue; 
                        }
                        else
                        {
                            BarcodeEntry.Text = "Nieobs³ugiwany kod"; 
                        }
                    }
                    else
                    {
                        await DisplayAlert("B³¹d", "Nie znaleziono kodu QR/kreskowego na obrazie.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("B³¹d", $"Nie uda³o siê zdekodowaæ kodu: {ex.Message}", "OK");
            }
        }


    }
}