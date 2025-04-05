using Microsoft.UI.Xaml.Data;
using System;

namespace PowerPad.WinUI.Converters
{
    public class IntToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int intValue)
            {
                return (double)intValue;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is double doubleValue)
            {
                return (int)doubleValue;
            }
            return 0;
        }
    }

    public class FloatToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is float floatValue)
            {
                return (double)floatValue;
            }
            return 0.0;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is double doubleValue)
            {
                return (float)doubleValue;
            }
            return 0.0f;
        }
    }
}
