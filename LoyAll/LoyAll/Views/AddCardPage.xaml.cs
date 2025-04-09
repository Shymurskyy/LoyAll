using LoyAll.Helper;
using LoyAll.Model;
using LoyAll.Services;
using LZStringCSharp;
using Newtonsoft.Json;
using ZXing.Net.Maui;
using Color = Microsoft.Maui.Graphics.Color;

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
                selectedImagePath = result.FullPath;

                string barcodeValue = await BarcodeHelper.DecodeBarcodeFromImage(stream);
                if (!string.IsNullOrEmpty(barcodeValue))
                {
                    if (barcodeValue.StartsWith("Q:#") || barcodeValue.StartsWith("B:#"))
                    {
                        BarcodeEntry.Text = barcodeValue;
                        ShowCodePreview(barcodeValue);
                    }
                    else
                    {
                        BarcodeEntry.Text = "Nieobs³ugiwany kod";
                    }
                }
            }
        }


        private async void OnSaveCardClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(StoreNameEntry.Text) || string.IsNullOrWhiteSpace(BarcodeEntry.Text))
            {
                using (CustomPopup errorPopup = new CustomPopup(false))
                {
                    errorPopup.SetTitle("B³¹d");
                    errorPopup.SetMessage("Podaj nazwê i zeskanuj kod");
                    errorPopup.AddOption("OK", () => { });
                    await errorPopup.ShowAsync(this);
                }
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

                ShowCodePreview(barcodeValue);
            }
        }
        private void ShowCodePreview(string barcodeValue)
        {
            if (barcodeValue.StartsWith("B:#"))
            {
                PreviewCodeImage.Source = BarcodeHelper.GenerateBarcode(barcodeValue.Replace("B:#", ""));
                PreviewCodeImage.WidthRequest = 400;
                PreviewCodeImage.HeightRequest = 200;
            }
            else if (barcodeValue.StartsWith("Q:#"))
            {
                PreviewCodeImage.Source = BarcodeHelper.GenerateQrCode(barcodeValue.Replace("Q:#", ""));
                PreviewCodeImage.WidthRequest = 200;
                PreviewCodeImage.HeightRequest = 200;
            }

            BarcodeEntry.Text = barcodeValue;
            AddButtonsView.IsVisible = false;
            CodePreviewView.IsVisible = true;
        }
        private async void OnImportSharedCardClicked(object sender, EventArgs e)
        {
            try
            {
                CustomPopup popup = new CustomPopup();
                popup.SetTitle("Wybierz sposób importu");

                popup.AddOption("Import z obrazu", async () =>
                {
                    FileResult? result = await MediaPicker.PickPhotoAsync();
                    if (result != null)
                    {
                        Stream stream = await result.OpenReadAsync();
                        string decodedValue = LZString.DecompressFromEncodedURIComponent(await BarcodeHelper.DecodeBarcodeFromImage(stream, true));

                        List<dynamic>? tempCards = JsonConvert.DeserializeObject<List<dynamic>>(decodedValue);
                        List<Card> importedCards = tempCards.Select(x => new Card
                        {
                            StoreName = x.n,
                            CardValue = x.k
                        }).ToList();

                        if (importedCards != null)
                        {
                            foreach (Card? item in importedCards)
                            {
                                CardStorageService.SaveCard(item);
                                mainPage.AddCard(item);
                            }
                        }
                    }
                    await Navigation.PopAsync();
                });

                popup.AddOption("Import z tekstu", async () =>
                {
                    CustomPopup textPopup = new CustomPopup();
                    textPopup.SetTitle("Wklej zakodowan¹ wiadomoœæ");
                    Entry entry = new Entry
                    {
                        Placeholder = "WprowadŸ tekst...",
                        Margin = new Thickness(0, 0, 0, 20),
                        TextColor = Color.FromArgb("#1A8CD8"),
                    };

                    textPopup.InsertView(1, entry);

                    textPopup.AddOption("Importuj", async () =>
                    {
                        string inputText = entry.Text;
                        if (!string.IsNullOrWhiteSpace(inputText))
                        {
                            try
                            {
                                string decodedValue = LZString.DecompressFromEncodedURIComponent(inputText);
                                List<dynamic>? tempCards = JsonConvert.DeserializeObject<List<dynamic>>(decodedValue);
                                List<Card> importedCards = tempCards.Select(x => new Card
                                {
                                    StoreName = x.n,
                                    CardValue = x.k
                                }).ToList();

                                if (importedCards != null)
                                {
                                    foreach (Card? item in importedCards)
                                    {
                                        CardStorageService.SaveCard(item);
                                        mainPage.AddCard(item);
                                    }
                                }
                                await Navigation.PopAsync();
                            }
                            catch (Exception ex)
                            {
                                using (CustomPopup errorPopup = new CustomPopup(false))
                                {
                                    errorPopup.SetTitle("B³¹d");
                                    errorPopup.SetMessage($"Nie uda³o siê zdekodowaæ wiadomoœci. Upewnij siê ¿e importujesz kod wygenerowany w tej aplikacji!");
                                    errorPopup.AddOption("OK", () => { });
                                    await errorPopup.ShowAsync(this);
                                }
                                return;
                            }
                        }
                    });

                    await textPopup.ShowAsync(this);
                });

                await popup.ShowAsync(this);
            }
            catch (Exception ex)
            {
                using (CustomPopup errorPopup = new CustomPopup(false))
                {
                    errorPopup.SetTitle("B³¹d");
                    errorPopup.SetMessage("Nie uda³o siê zdekodowaæ kodu. Upewnij siê ¿e importujesz kod wygenerowany w tej aplikacji!");
                    errorPopup.AddOption("OK", () => { });
                    await errorPopup.ShowAsync(this);
                }
                return;
            }
        }


        private async void OnScanBarcodeClicked(object sender, EventArgs e)
        {
            try
            {
                FileResult? photo = await MediaPicker.CapturePhotoAsync();

                if (photo != null)
                {
                    Stream photoStream = await photo.OpenReadAsync();

                    string barcodeValue = await BarcodeHelper.DecodeBarcodeFromImage(photoStream);

                    if (!string.IsNullOrEmpty(barcodeValue))
                    {
                        if (barcodeValue.StartsWith("Q:#") || barcodeValue.StartsWith("B:#"))
                        {
                            BarcodeEntry.Text = barcodeValue;
                            ShowCodePreview(barcodeValue);
                        }
                        else
                        {
                            BarcodeEntry.Text = "Nieobs³ugiwany kod";
                        }
                    }
                    else
                    {
                        using (CustomPopup errorPopup = new CustomPopup(false))
                        {
                            errorPopup.SetTitle("B³¹d");
                            errorPopup.SetMessage("Nie znaleziono kodu QR/kreskowego na obrazie.");
                            errorPopup.AddOption("OK", () => { });
                            await errorPopup.ShowAsync(this);
                        }
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                using (CustomPopup errorPopup = new CustomPopup(false))
                {
                    errorPopup.SetTitle("B³¹d");
                    errorPopup.SetMessage($"Nie uda³o siê zdekodowaæ kodu: {ex.Message}");
                    errorPopup.AddOption("OK", () => { });
                    await errorPopup.ShowAsync(this);
                }
                return;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ResetForm();
        }
        private void ResetForm()
        {
            AddButtonsView.IsVisible = true;
            StoreNameEntry.Text = string.Empty;
            BarcodeEntry.Text = string.Empty;
        }
    }
}