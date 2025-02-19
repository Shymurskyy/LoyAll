using QRCoder;
using SkiaSharp;
using ZXing.SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;

namespace LoyAll.Helper
{
    public static class CodeGeneratorHelper
    {
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
