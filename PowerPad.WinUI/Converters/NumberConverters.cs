using Microsoft.UI.Xaml.Data;
using System;

namespace PowerPad.WinUI.Converters
{
    /// <summary>
    /// Converter class to convert an integer value to a double and vice versa.
    /// Implements the IValueConverter interface for use in XAML data binding.
    /// </summary>
    public class IntToDoubleConverter : IValueConverter
    {
        /// <summary>
        /// Converts an integer value to a double.
        /// </summary>
        /// <param name="value">The value to convert, expected to be of type int.</param>
        /// <param name="targetType">The target type of the conversion (not used).</param>
        /// <param name="parameter">Optional parameter for the conversion (not used).</param>
        /// <param name="language">The culture language for the conversion (not used).</param>
        /// <returns>A double representation of the input integer, or 0.0 if the input is not an integer.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int intValue)
            {
                return (double)intValue;
            }
            return 0.0;
        }

        /// <summary>
        /// Converts a double value back to an integer.
        /// </summary>
        /// <param name="value">The value to convert, expected to be of type double.</param>
        /// <param name="targetType">The target type of the conversion (not used).</param>
        /// <param name="parameter">Optional parameter for the conversion (not used).</param>
        /// <param name="language">The culture language for the conversion (not used).</param>
        /// <returns>An integer representation of the input double, or 0 if the input is not a double.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is double doubleValue)
            {
                return (int)doubleValue;
            }
            return 0;
        }
    }

    /// <summary>
    /// Converter class to convert a float value to a double and vice versa.
    /// Implements the IValueConverter interface for use in XAML data binding.
    /// </summary>
    public class FloatToDoubleConverter : IValueConverter
    {
        /// <summary>
        /// Converts a float value to a double.
        /// </summary>
        /// <param name="value">The value to convert, expected to be of type float.</param>
        /// <param name="targetType">The target type of the conversion (not used).</param>
        /// <param name="parameter">Optional parameter for the conversion (not used).</param>
        /// <param name="language">The culture language for the conversion (not used).</param>
        /// <returns>A double representation of the input float, or 0.0 if the input is not a float.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is float floatValue)
            {
                return (double)floatValue;
            }
            return 0.0;
        }

        /// <summary>
        /// Converts a double value back to a float.
        /// </summary>
        /// <param name="value">The value to convert, expected to be of type double.</param>
        /// <param name="targetType">The target type of the conversion (not used).</param>
        /// <param name="parameter">Optional parameter for the conversion (not used).</param>
        /// <param name="language">The culture language for the conversion (not used).</param>
        /// <returns>A float representation of the input double, or 0.0f if the input is not a double.</returns>
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
