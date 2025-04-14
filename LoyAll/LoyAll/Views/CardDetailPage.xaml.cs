using LoyAll.Helper;
using LoyAll.Model;
using The49.Maui.BottomSheet;

namespace LoyAll
{
    public partial class CardDetailPage : BottomSheet
    {
        private readonly Card _card;
        private bool _isDisposed;
        private readonly SemaphoreSlim _initSemaphore = new(1, 1);

        public CardDetailPage(Card card)
        {
            InitializeComponent();
            _card = card;

            this.HasBackdrop = true;
            this.BackgroundColor = Colors.White;
            this.Dismissed += OnDismissed;

            StoreNameLabel.Text = _card.StoreName;
            RealCardValueLabel.Text = _card.CleanCardValue;
            //BarcodeSwitch.IsToggled = _card.CardValue.StartsWith("B:#");

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                await _initSemaphore.WaitAsync();
                if (_isDisposed) return;

                await Task.Delay(100); 

                if (_card.CardValue.StartsWith("B:#"))
                {
                    await LoadBarcodeAsync();
                }
                else
                {
                    await LoadQrCodeAsync();
                }
            }
            finally
            {
                _initSemaphore.Release();
            }
        }

        private async Task LoadBarcodeAsync()
        {
            var barcode = await Task.Run(() =>
                BarcodeHelper.GenerateBarcode(_card.CleanCardValue));

            if (!_isDisposed)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    CodeImage.WidthRequest = 400;
                    CodeImage.HeightRequest = 150;
                    CodeImage.Source = barcode;
                });
            }
        }

        private async Task LoadQrCodeAsync()
        {
            var qrCode = await Task.Run(() =>
                BarcodeHelper.GenerateQrCode(_card.CleanCardValue));

            if (!_isDisposed)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    CodeImage.WidthRequest = 200;
                    CodeImage.HeightRequest = 200;
                    CodeImage.Source = qrCode;
                });
            }
        }

        private async void OnBarcodeSwitchToggled(object sender, ToggledEventArgs e)
        {
            try
            {
                await _initSemaphore.WaitAsync();
                if (_isDisposed) return;

                if (e.Value)
                {
                    await LoadBarcodeAsync();
                }
                else
                {
                    await LoadQrCodeAsync();
                }
            }
            finally
            {
                _initSemaphore.Release();
            }
        }

        private void OnDismissed(object sender, DismissOrigin e)
        {
            _isDisposed = true;
            this.Dismissed -= OnDismissed;
        }
    }
}