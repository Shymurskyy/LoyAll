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

            // Domyœlnie pokazujemy QR Code
            CodeImage.Source = GenerateQrCode(_card.CardValue);
        }

        private void OnBarcodeSwitchToggled(object sender, ToggledEventArgs e)
        {
            if (e.Value)
            {
                CodeImage.Source = GenerateBarcode(_card.CardValue);
                CodeImage.WidthRequest = 400; 
                CodeImage.HeightRequest = 400;
            }
            else
            {
                CodeImage.Source = GenerateQrCode(_card.CardValue);
                CodeImage.WidthRequest = 200;
                CodeImage.HeightRequest = 200;
            }
        }

        public static ImageSource GenerateQrCode(string value, int size = 200)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(value, QRCodeGenerator.ECCLevel.L);
            BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(10);

            using (SKBitmap skBitmap = SKBitmap.Decode(qrCodeImage))
            using (SKImage skImage = SKImage.FromBitmap(skBitmap))
            using (SKData data = skImage.Encode(SKEncodedImageFormat.Png, 100))
            {
                MemoryStream stream = new MemoryStream(data.ToArray());
                return ImageSource.FromStream(() => stream);
            }
        }

        public static ImageSource GenerateBarcode(string value)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = 1600,
                    Height = 400,
                    Margin = 40
                }
            };

            var bitmap = writer.Write(value);

            using (var image = SKImage.FromBitmap(bitmap))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            {
                MemoryStream stream = new MemoryStream(data.ToArray());
                return ImageSource.FromStream(() => stream);
            }
        }

    }
}
