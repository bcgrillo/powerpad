using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Data;
using System;

namespace PowerPad.WinUI.Converters
{
    /// <summary>
    /// Converts a boolean value to a corresponding FontWeight.
    /// Implements the IValueConverter interface for use in XAML data binding.
    /// </summary>
    public class BooleanToFontWeightConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to a FontWeight.
        /// </summary>
        /// <param name="value">The value to convert. Expected to be a boolean.</param>
        /// <param name="targetType">The target type of the conversion. Not used in this implementation.</param>
        /// <param name="parameter">An optional parameter for the conversion. Not used in this implementation.</param>
        /// <param name="language">The language of the conversion. Not used in this implementation.</param>
        /// <returns>Returns FontWeights.SemiBold if the value is true; otherwise, FontWeights.Normal.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool booleanValue)
            {
                return booleanValue ? FontWeights.SemiBold : FontWeights.Normal;
            }

            return FontWeights.Normal;
        }

        /// <summary>
        /// Not implemented. Converts a FontWeight back to a boolean value.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
