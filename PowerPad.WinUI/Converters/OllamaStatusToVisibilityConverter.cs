using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;
using System;
using PowerPad.Core.Models.AI;

namespace PowerPad.WinUI.Converters
{
    public class OllamaOnlineStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is OllamaStatus status)
            {
                return status == OllamaStatus.Online ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class OllamaAvailableStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is OllamaStatus status)
            {
                return status == OllamaStatus.Available ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
