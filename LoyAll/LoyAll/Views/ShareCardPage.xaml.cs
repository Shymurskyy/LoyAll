using LoyAll.Helper;
using QRCoder;
using SkiaSharp;

public class ShareCardPage : ContentPage
{
    public ShareCardPage(string compressedData)
    {
        Title = "Udostêpnij swoje karty";
        var qrImage = new Image { Source = CodeGeneratorHelper.GenerateQrCode(compressedData), WidthRequest = 200, HeightRequest = 200 };
        Content = new StackLayout
        {
            Padding = 20,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Children = { qrImage }
        };
    }
}