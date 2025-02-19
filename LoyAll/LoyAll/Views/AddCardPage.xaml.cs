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

                string barcodeValue = await DecodeBarcodeFromImage(stream);
                if (!string.IsNullOrEmpty(barcodeValue))
                {
                    BarcodeEntry.Text = barcodeValue;
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
                BarcodeReader barcodeReader = new BarcodeReader()
                {
                    Options = new ZXing.Common.DecodingOptions()
                    {
                        TryHarder = true,
                        PossibleFormats = new List<ZXing.BarcodeFormat>()
                        {
                        ZXing.BarcodeFormat.QR_CODE,
                        ZXing.BarcodeFormat.CODE_128,
                        ZXing.BarcodeFormat.CODE_39,
                        ZXing.BarcodeFormat.EAN_13,
                        ZXing.BarcodeFormat.UPC_A
                        }
                    }
                };

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await imageStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    using (SKBitmap skBitmap = SKBitmap.Decode(memoryStream))
                    {
                        if (skBitmap == null)
                        {
                            await DisplayAlert("B³¹d", "Nie uda³o siê wczytaæ obrazu.", "OK");
                            return null;
                        }

                        Result barcodeResult = barcodeReader.Decode(skBitmap);
                        return barcodeResult?.Text;
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


                FileResult? result = await MediaPicker.PickPhotoAsync();
                if (result != null)
                {
                    Stream stream = await result.OpenReadAsync();
                    string decodedValue = LZString.DecompressFromEncodedURIComponent(await DecodeBarcodeFromImage(stream));
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
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("B³¹d", $"Nie uda³o siê zdekodowaæ kodu: {ex.Message}", "OK");
                return;
            }
        }

        private void OnScanBarcodeClicked(object sender, EventArgs e)
        {
            barcodeReader.IsVisible = true;
            barcodeReader.IsDetecting = true;
        }
    }
}