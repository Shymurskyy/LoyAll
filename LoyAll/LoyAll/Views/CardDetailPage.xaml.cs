using LoyAll.Helper;
using LoyAll.Model;
using QRCoder;
using SkiaSharp;
using ZXing;
using ZXing.SkiaSharp;

namespace LoyAll
{
    public partial class CardDetailPage : ContentPage
    {
        private readonly Card _card;

        public CardDetailPage(Card card)
        {
            InitializeComponent();
            _card = card;

            StoreNameLabel.Text = _card.StoreName;
            CardImage.Source = ImageSource.FromFile(_card.CardValue);

            CodeImage.Source = CodeGeneratorHelper.GenerateQrCode(_card.CardValue);
        }

        private void OnBarcodeSwitchToggled(object sender, ToggledEventArgs e)
        {
            if (e.Value)
            {
                CodeImage.WidthRequest = 400; 
                CodeImage.HeightRequest = 400;
                CodeImage.Source = CodeGeneratorHelper.GenerateBarcode(_card.CardValue);
            }
            else
            {
                CodeImage.WidthRequest = 200;
                CodeImage.HeightRequest = 200;
                CodeImage.Source = CodeGeneratorHelper.GenerateQrCode(_card.CardValue);
            }
        }

    }
}
