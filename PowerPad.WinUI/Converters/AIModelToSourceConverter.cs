using Microsoft.UI.Xaml.Data;
using PowerPad.WinUI.Helpers;
using PowerPad.WinUI.ViewModels.AI;
using System;

namespace PowerPad.WinUI.Converters
{
    public class AIModelToSourceConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            var model = (AIModelViewModel?)value;

            return model?.ModelProvider.GetIcon();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
