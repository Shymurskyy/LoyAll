using LoyAll.Model;
using LoyAll.Services;
using ZXing;
using ZXing.Net.Maui;
using ZXing.SkiaSharp;
using SkiaSharp;

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

                // Dekoduj kod QR/kreskowy z wybranego obrazu
                string barcodeValue = await DecodeBarcodeFromImage(stream);
                if (!string.IsNullOrEmpty(barcodeValue))
                {
                    BarcodeEntry.Text = barcodeValue; // Wyœwietl wartoœæ kodu w polu Entry
                }
                else
                {
                    await DisplayAlert("B³¹d", "Nie znaleziono kodu QR/kreskowego na obrazie.", "OK");
                }
            }
        }

        private async Task<string> DecodeBarcodeFromImage(Stream imageStream)
        {
            try
            {
                // Utwórz dekoder
                var barcodeReader = new BarcodeReader();

                // Przekonwertuj strumieñ na SKBitmap
                using (var memoryStream = new MemoryStream())
                {
                    await imageStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    // Wczytaj obraz jako SKBitmap
                    using (var skBitmap = SKBitmap.Decode(memoryStream))
                    {
                        if (skBitmap == null)
                        {
                            await DisplayAlert("B³¹d", "Nie uda³o siê wczytaæ obrazu.", "OK");
                            return null;
                        }

                        // Dekoduj obraz
                        var barcodeResult = barcodeReader.Decode(skBitmap);
                        return barcodeResult?.Text; // Zwróæ wartoœæ kodu
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("B³¹d", $"Nie uda³o siê zdekodowaæ kodu: {ex.Message}", "OK");
                return null;
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
            var firstBarcode = e.Results.FirstOrDefault();
            if (firstBarcode != null)
            {
                string barcodeValue = firstBarcode.Value;
                // Przypisz wartoœæ kodu do pola Entry
                BarcodeEntry.Text = barcodeValue;
            }
        }

        private void OnScanBarcodeClicked(object sender, EventArgs e)
        {
            barcodeReader.IsVisible = true;
            barcodeReader.IsDetecting = true;
        }
    }
}