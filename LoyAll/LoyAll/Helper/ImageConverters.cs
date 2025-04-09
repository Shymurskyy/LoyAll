using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoyAll.Helper
{
    public class CardTypeToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string cardValue)
            {
                return cardValue.StartsWith("Q:#") ? "qr_placeholder.png" : "barcode_placeholder.png";
            }
            return "blue_barcode_placeholder.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
