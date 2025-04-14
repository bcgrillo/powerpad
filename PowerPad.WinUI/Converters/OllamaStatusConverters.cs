using Microsoft.UI.Xaml.Data;
using System;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using PowerPad.Core.Models.AI;

namespace PowerPad.WinUI.Converters
{
    public class OllamaStatusToColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var ollamaStatus = (OllamaStatus)value;

            return ollamaStatus switch
            {
                OllamaStatus.Online => new SolidColorBrush(Colors.Green),
                OllamaStatus.Available => new SolidColorBrush(Colors.Orange),
                OllamaStatus.Error => new SolidColorBrush(Colors.Red),
                OllamaStatus.Unreachable => new SolidColorBrush(Colors.Orange),
                OllamaStatus.Updating => new SolidColorBrush(Colors.Orange),
                _ => new SolidColorBrush(Colors.Gray),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class OllamaStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var ollamaStatus = (OllamaStatus)value;

            var result = ollamaStatus switch
            {
                OllamaStatus.Online => "Conectado",
                OllamaStatus.Available => "Disponible",
                OllamaStatus.Error => "Error",
                OllamaStatus.Unreachable => "Inalcanzable",
                OllamaStatus.Unknown => "Desconocido",
                OllamaStatus.Updating => "Actualizando",
                _ => string.Empty,
            };

            if (parameter as bool? == true)
            {
                result += ollamaStatus switch
                {
                    OllamaStatus.Available => " (no iniciado)",
                    OllamaStatus.Error => " (error al iniciar)",
                    OllamaStatus.Unreachable => " (no responde)",
                    OllamaStatus.Updating => " (espere)",
                    _ => string.Empty,
                };
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class OllamaStatusOnlineToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is OllamaStatus status)
            {
                return status == OllamaStatus.Online;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
