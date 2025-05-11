using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using PowerPad.Core.Models.AI;
using System;

namespace PowerPad.WinUI.Converters
{
    /// <summary>
    /// Converts a <see cref="ServiceStatus"/> value to a corresponding <see cref="SolidColorBrush"/>.
    /// Implements the IValueConverter interface for use in XAML data binding.
    /// </summary>
    public class ServiceStatusToColorBrushConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="ServiceStatus"/> value to a <see cref="SolidColorBrush"/>.
        /// </summary>
        /// <param name="value">The <see cref="ServiceStatus"/> value to convert.</param>
        /// <param name="targetType">The target type of the conversion (not used).</param>
        /// <param name="parameter">Optional parameter for the conversion (not used).</param>
        /// <param name="language">The language of the conversion (not used).</param>
        /// <returns>A <see cref="SolidColorBrush"/> corresponding to the <see cref="ServiceStatus"/> value.</returns>
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
                ServiceStatus.NotFound => new SolidColorBrush(Colors.Red),
                _ => (Brush)Application.Current.Resources["TextFillColorTertiaryBrush"],
            };
        }

        /// <summary>
        /// Not implemented. Converts back from a <see cref="SolidColorBrush"/> to a <see cref="ServiceStatus"/>.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a <see cref="ServiceStatus"/> value to a corresponding string representation.
    /// Implements the IValueConverter interface for use in XAML data binding.
    /// </summary>
    public class ServiceStatusToStringConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="ServiceStatus"/> value to a string.
        /// </summary>
        /// <param name="value">The <see cref="ServiceStatus"/> value to convert.</param>
        /// <param name="targetType">The target type of the conversion (not used).</param>
        /// <param name="parameter">Optional parameter to append additional information to the string.</param>
        /// <param name="language">The language of the conversion (not used).</param>
        /// <returns>A string representation of the <see cref="ServiceStatus"/> value.</returns>
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

        /// <summary>
        /// Not implemented. Converts back from a string to a <see cref="ServiceStatus"/>.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a <see cref="ServiceStatus"/> value to a boolean indicating equality with a specified status.
    /// Implements the IValueConverter interface for use in XAML data binding.
    /// </summary>
    public class ServiceStatusToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="ServiceStatus"/> value to a boolean.
        /// </summary>
        /// <param name="value">The <see cref="ServiceStatus"/> value to convert.</param>
        /// <param name="targetType">The target type of the conversion (not used).</param>
        /// <param name="parameter">The <see cref="ServiceStatus"/> value to compare against.</param>
        /// <param name="language">The language of the conversion (not used).</param>
        /// <returns>True if the <see cref="ServiceStatus"/> matches the parameter; otherwise, false.</returns>
        public object Convert(object value, Type targetType, object? parameter, string language)
        {
            Enum.TryParse(typeof(ServiceStatus), parameter as string, out object? enumParam);

            if (value is ServiceStatus status)
            {
                return status == (enumParam as ServiceStatus? ?? ServiceStatus.Online);
            }
            return false;
        }

        /// <summary>
        /// Not implemented. Converts back from a boolean to a <see cref="ServiceStatus"/>.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object? parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a <see cref="ServiceStatus"/> value to a boolean indicating inequality with a specified status.
    /// Implements the IValueConverter interface for use in XAML data binding.
    /// </summary>
    public class ServiceStatusToBoolNegationConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="ServiceStatus"/> value to a boolean.
        /// </summary>
        /// <param name="value">The <see cref="ServiceStatus"/> value to convert.</param>
        /// <param name="targetType">The target type of the conversion (not used).</param>
        /// <param name="parameter">The <see cref="ServiceStatus"/> value to compare against.</param>
        /// <param name="language">The language of the conversion (not used).</param>
        /// <returns>True if the <see cref="ServiceStatus"/> does not match the parameter; otherwise, false.</returns>
        public object Convert(object value, Type targetType, object? parameter, string language)
        {
            Enum.TryParse(typeof(ServiceStatus), parameter as string, out object? enumParam);

            if (value is ServiceStatus status)
            {
                return status != (enumParam as ServiceStatus? ?? ServiceStatus.Online);
            }
            return false;
        }

        /// <summary>
        /// Not implemented. Converts back from a boolean to a <see cref="ServiceStatus"/>.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object? parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a <see cref="ServiceStatus"/> value to a <see cref="Visibility"/> value.
    /// Implements the IValueConverter interface for use in XAML data binding.
    /// </summary>
    public class ServiceStatusToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="ServiceStatus"/> value to a <see cref="Visibility"/>.
        /// </summary>
        /// <param name="value">The <see cref="ServiceStatus"/> value to convert.</param>
        /// <param name="targetType">The target type of the conversion (not used).</param>
        /// <param name="parameter">The <see cref="ServiceStatus"/> value to compare against.</param>
        /// <param name="language">The language of the conversion (not used).</param>
        /// <returns><see cref="Visibility.Visible"/> if the <see cref="ServiceStatus"/> matches the parameter; otherwise, <see cref="Visibility.Collapsed"/>.</returns>
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

        /// <summary>
        /// Not implemented. Converts back from a <see cref="Visibility"/> to a <see cref="ServiceStatus"/>.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object? parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a <see cref="ServiceStatus"/> value to a negated <see cref="Visibility"/> value.
    /// Implements the IValueConverter interface for use in XAML data binding.
    /// </summary>
    public class ServiceStatusToVisibilityNegationConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="ServiceStatus"/> value to a negated <see cref="Visibility"/>.
        /// </summary>
        /// <param name="value">The <see cref="ServiceStatus"/> value to convert.</param>
        /// <param name="targetType">The target type of the conversion (not used).</param>
        /// <param name="parameter">The <see cref="ServiceStatus"/> value to compare against.</param>
        /// <param name="language">The language of the conversion (not used).</param>
        /// <returns><see cref="Visibility.Visible"/> if the <see cref="ServiceStatus"/> does not match the parameter; otherwise, <see cref="Visibility.Collapsed"/>.</returns>
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

        /// <summary>
        /// Not implemented. Converts back from a <see cref="Visibility"/> to a <see cref="ServiceStatus"/>.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object? parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
