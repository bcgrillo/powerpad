using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;
using System;
using Microsoft.UI.Text;

namespace PowerPad.WinUI.Converters
{
    public class BooleanToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool booleanValue)
            {
                return booleanValue ? FontWeights.SemiBold : FontWeights.Normal;
            }

            return FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
