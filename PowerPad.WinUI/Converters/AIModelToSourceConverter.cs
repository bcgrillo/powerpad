using Microsoft.UI.Xaml.Data;
using PowerPad.WinUI.Helpers;
using PowerPad.WinUI.ViewModels.AI;
using System;

namespace PowerPad.WinUI.Converters
{
    /// <summary>
    /// Converts an AIModelViewModel instance to a corresponding source icon.
    /// Implements the IValueConverter interface for use in XAML data binding.
    /// </summary>
    public class AIModelToSourceConverter : IValueConverter
    {
        /// <summary>
        /// Converts an AIModelViewModel object to its associated icon source.
        /// </summary>
        /// <param name="value">The AIModelViewModel instance to convert.</param>
        /// <param name="targetType">The target type of the binding (not used).</param>
        /// <param name="parameter">Optional parameter for the converter (not used).</param>
        /// <param name="language">The language of the conversion (not used).</param>
        /// <returns>The icon source associated with the AIModelViewModel, or null if the model is null.</returns>
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            var model = (AIModelViewModel?)value;

            return model?.ModelProvider.GetIcon();
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
