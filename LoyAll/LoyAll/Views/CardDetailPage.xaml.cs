using LoyAll.Helper;
using LoyAll.Model;
using The49.Maui.BottomSheet;

namespace LoyAll
{
    public partial class CardDetailPage : BottomSheet
    {
        private readonly Card _card;

        public CardDetailPage(Card card)
        {
            InitializeComponent();
            _card = card;

            this.HasBackdrop = true;
            this.BackgroundColor = Colors.White;
            

            StoreNameLabel.Text = _card.StoreName;

            if (_card.CardValue.StartsWith("B:#"))
            {
                CodeImage.Source = BarcodeHelper.GenerateBarcode(_card.CleanCardValue);
                BarcodeSwitch.IsToggled = true;
                CodeImage.WidthRequest = 400;
                CodeImage.HeightRequest = 150;
            }
            else
            {
                CodeImage.Source = BarcodeHelper.GenerateQrCode(_card.CleanCardValue);
                BarcodeSwitch.IsToggled = false;
                CodeImage.WidthRequest = 200;
                CodeImage.HeightRequest = 200;
            }

            RealCardValueLabel.Text = _card.CleanCardValue;
        }

        private void OnBarcodeSwitchToggled(object sender, ToggledEventArgs e)
        {
            if (e.Value)
            {
                CodeImage.WidthRequest = 400;
                CodeImage.HeightRequest = 150;
                CodeImage.Source = BarcodeHelper.GenerateBarcode(_card.CleanCardValue);
            }
            else
            {
                CodeImage.WidthRequest = 200;
                CodeImage.HeightRequest = 200;
                CodeImage.Source = BarcodeHelper.GenerateQrCode(_card.CleanCardValue);
            }
           
        }
        
    }
}