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
    public static class BarcodeHelper
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
        public static ImageSource GeneratePDF417Code(string value)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.PDF_417,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = 1600,
                    Height = 400,
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

        public static async Task<string> DecodeBarcodeFromImage(Stream imageStream, bool isImportShare = false)
        {
            try
            {
                BarcodeReader barcodeReader = new BarcodeReader()
                {
                    Options = new ZXing.Common.DecodingOptions()
                    {
                        TryHarder = true,
                        PossibleFormats = new List<ZXing.BarcodeFormat>()
                        {
                            ZXing.BarcodeFormat.QR_CODE,
                            ZXing.BarcodeFormat.CODE_128,
                            ZXing.BarcodeFormat.CODE_39,
                            ZXing.BarcodeFormat.EAN_13,
                            ZXing.BarcodeFormat.UPC_A,
                            ZXing.BarcodeFormat.PDF_417
                        }
                    }
                };

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await imageStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    using (SKBitmap skBitmap = SKBitmap.Decode(memoryStream))
                    {
                        if (skBitmap == null)
                        {
                            return null;
                        }

                        SKBitmap scaledBitmap = skBitmap;
                        if (skBitmap.Width > 1500 || skBitmap.Height > 1500)
                        {
                            int newWidth = 1500;
                            int newHeight = 1500;
                            if (skBitmap.Width > skBitmap.Height)
                                newHeight = (int)(skBitmap.Height * (1500.0 / skBitmap.Width));
                            else
                                newWidth = (int)(skBitmap.Width * (1500.0 / skBitmap.Height));

                            scaledBitmap = skBitmap.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.Medium);
                        }

                        using (MemoryStream scaledMemoryStream = new MemoryStream())
                        {
                            using (SKImage img = SKImage.FromBitmap(scaledBitmap))
                            {
                                img.Encode(SKEncodedImageFormat.Png, 100).SaveTo(scaledMemoryStream);
                            }

                            scaledMemoryStream.Position = 0;

                            using (SKBitmap finalBitmap = SKBitmap.Decode(scaledMemoryStream))
                            {
                                Result barcodeResult = barcodeReader.Decode(finalBitmap);
                                if (barcodeResult != null)
                                {
                                    if (barcodeResult.BarcodeFormat == ZXing.BarcodeFormat.QR_CODE)
                                        return isImportShare ? barcodeResult.Text : "Q:#" + barcodeResult.Text;

                                    else if (barcodeResult.BarcodeFormat == ZXing.BarcodeFormat.CODE_128 ||
                                             barcodeResult.BarcodeFormat == ZXing.BarcodeFormat.CODE_39 ||
                                             barcodeResult.BarcodeFormat == ZXing.BarcodeFormat.EAN_13 ||
                                             barcodeResult.BarcodeFormat == ZXing.BarcodeFormat.UPC_A)
                                        return isImportShare ? barcodeResult.Text : "B:#" + barcodeResult.Text;

                                    else if (barcodeResult.BarcodeFormat == ZXing.BarcodeFormat.PDF_417)
                                        return isImportShare ? barcodeResult.Text : "P:#" + barcodeResult.Text;
                                }
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
