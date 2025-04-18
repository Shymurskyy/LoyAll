using CommunityToolkit.Maui.Views;
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
            await ExecuteImageClicked();
        }

        private async Task ExecuteImageClicked()
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
                        BarcodeEntry.Text = LanguageHelper.Instance["UnsupportedCode"];
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
                    errorPopup.SetTitle(LanguageHelper.Instance["ErrorTitle"]);
                    errorPopup.SetMessage(LanguageHelper.Instance["EnterNameAndScanError"]);
                    errorPopup.AddOption(LanguageHelper.Instance["OKButton"], () => { });
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
                popup.SetTitle(LanguageHelper.Instance["ChooseImportMethod"]);

                popup.AddOption(LanguageHelper.Instance["ImportFromImage"], async () =>
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

                popup.AddOption(LanguageHelper.Instance["ImportFromText"], async () =>
                {
                    CustomPopup textPopup = new CustomPopup();
                    textPopup.SetTitle(LanguageHelper.Instance["PasteEncodedMessage"]);
                    Entry entry = new Entry
                    {
                        Placeholder = LanguageHelper.Instance["EnterTextPlaceholder"],
                        Margin = new Thickness(0, 0, 0, 20),
                        TextColor = Color.FromArgb("#1A8CD8"),
                    };

                    textPopup.InsertView(1, entry);

                    textPopup.AddOption(LanguageHelper.Instance["ImportButton"], async () =>
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
                                    errorPopup.SetTitle(LanguageHelper.Instance["ErrorTitle"]);
                                    errorPopup.SetMessage(LanguageHelper.Instance["DecodeErrorMessage"]);
                                    errorPopup.AddOption(LanguageHelper.Instance["OKButton"], () => { });
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
                    errorPopup.SetTitle(LanguageHelper.Instance["ErrorTitle"]);
                    errorPopup.SetMessage(LanguageHelper.Instance["DecodeErrorMessage"]);
                    errorPopup.AddOption(LanguageHelper.Instance["OKButton"], () => { });
                    await errorPopup.ShowAsync(this);
                }
                return;
            }
        }
        private async void OnScanBarcodeClicked(object sender, EventArgs e)
        {
            await ExecuteBarcodeClicked();
        }

        private async Task ExecuteBarcodeClicked()
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
                            BarcodeEntry.Text = LanguageHelper.Instance["UnsupportedCode"];
                        }
                    }
                    else
                    {
                        using (CustomPopup errorPopup = new CustomPopup(false))
                        {
                            errorPopup.SetTitle(LanguageHelper.Instance["ErrorTitle"]);
                            errorPopup.SetMessage(LanguageHelper.Instance["NoBarcodeFound"]);
                            errorPopup.AddOption(LanguageHelper.Instance["OKButton"], () => { });
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
                    errorPopup.SetTitle(LanguageHelper.Instance["ErrorTitle"]);
                    errorPopup.SetMessage($"{LanguageHelper.Instance["DecodeErrorPrefix"]} {ex.Message}");
                    errorPopup.AddOption(LanguageHelper.Instance["OKButton"], () => { });
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
        private async void OnAddClicked(object sender, EventArgs e)
        {
            using (CustomPopup customPopup = new CustomPopup())
            {
                customPopup.SetTitle(LanguageHelper.Instance["AddNewCard"]);
                customPopup.AddOption(LanguageHelper.Instance["AddFromGallery"], async () =>
                {
                    await ExecuteImageClicked();
                });
                customPopup.AddOption(LanguageHelper.Instance["ScanCode"], async () => { await ExecuteBarcodeClicked(); });
                await customPopup.ShowAsync(this);
            }
        }
    }
}