using Microsoft.UI.Xaml.Data;
using System;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using PowerPad.Core.Models.AI;

namespace PowerPad.WinUI.Converters
{
    public class OllamaOnlineStatusToColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var ollamaStatus = (OllamaStatus)value;

            return ollamaStatus switch
            {
                OllamaStatus.Online => new SolidColorBrush(Colors.Green),
                OllamaStatus.Available => new SolidColorBrush(Colors.Orange),
                OllamaStatus.Error => new SolidColorBrush(Colors.Red),
                _ => new SolidColorBrush(Colors.Gray),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
