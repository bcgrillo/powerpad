using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using PowerPad.Core.Models.AI;
using System;

namespace PowerPad.WinUI.Converters
{
    public class ServiceStatusToColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var ollamaStatus = (ServiceStatus)value;

            return ollamaStatus switch
            {
                ServiceStatus.Unconfigured => new SolidColorBrush(Colors.Orange),
                ServiceStatus.Updating => new SolidColorBrush(Colors.Orange),
                ServiceStatus.Available => new SolidColorBrush(Colors.Orange),
                ServiceStatus.Online => new SolidColorBrush(Colors.Green),
                ServiceStatus.Error => new SolidColorBrush(Colors.Red),
                _ => new SolidColorBrush(Colors.Gray),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ServiceStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var ollamaStatus = (ServiceStatus)value;

            var result = ollamaStatus switch
            {
                ServiceStatus.Unknown => "Desconocido",
                ServiceStatus.Unconfigured => "No configurado",
                ServiceStatus.Updating => "Actualizando",
                ServiceStatus.Available => "Disponible",
                ServiceStatus.Online => "Conectado",
                ServiceStatus.Error => "Error",
                ServiceStatus.NotFound => "No encontrado",
                _ => string.Empty,
            };

            if (parameter as bool? == true)
            {
                result += ollamaStatus switch
                {
                    ServiceStatus.Updating => " (espere)",
                    ServiceStatus.Available => " (no iniciado)",
                    ServiceStatus.Error => " (error al conectar)",
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

    public class ServiceStatusToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object? parameter, string language)
        {
            Enum.TryParse(typeof(ServiceStatus), parameter as string, out object? enumParam);

            if (value is ServiceStatus status)
            {
                return status == (enumParam as ServiceStatus? ?? ServiceStatus.Online);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object? parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ServiceStatusToBoolNegationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object? parameter, string language)
        {
            Enum.TryParse(typeof(ServiceStatus), parameter as string, out object? enumParam);

            if (value is ServiceStatus status)
            {
                return status != (enumParam as ServiceStatus? ?? ServiceStatus.Online);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object? parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ServiceStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object? parameter, string language)
        {
            Enum.TryParse(typeof(ServiceStatus), parameter as string, out object? enumParam);

            if (value is ServiceStatus status)
            {
                return status == (enumParam as ServiceStatus? ?? ServiceStatus.Online)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object? parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ServiceStatusToVisibilityNegationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object? parameter, string language)
        {
            Enum.TryParse(typeof(ServiceStatus), parameter as string, out object? enumParam);

            if (value is ServiceStatus status)
            {
                return status != (enumParam as ServiceStatus? ?? ServiceStatus.Online)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object? parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
