using LoyAll.Model;
using QRCoder;
using SkiaSharp;

namespace LoyAll
{
    public partial class CardDetailPage : ContentPage
    {
        public CardDetailPage(Card card)
        {
            InitializeComponent();
            StoreNameLabel.Text = card.StoreName;
            CardImage.Source = ImageSource.FromFile(card.CardValue);

            if (!string.IsNullOrEmpty(card.CardValue))
                QrCodeImage.Source = GenerateQrCode(card.CardValue);
            else
                QrCodeImage.Source = GenerateQrCode("Brak danych");
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
    }
}