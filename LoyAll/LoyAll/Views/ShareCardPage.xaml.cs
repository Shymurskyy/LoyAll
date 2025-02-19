using QRCoder;
using SkiaSharp;

public class ShareCardPage : ContentPage
{
    public ShareCardPage(string compressedData)
    {
        Title = "Udostêpnij swoje karty";
        var qrImage = new Image { Source = GenerateQrCode(compressedData), WidthRequest = 200, HeightRequest = 200 };
        Content = new StackLayout
        {
            Padding = 20,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Children = { qrImage }
        };
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