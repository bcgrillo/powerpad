using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using PowerPad.Core.Models.AI;
using System;
using PowerPad.WinUI.ViewModels.AI;
using PowerPad.WinUI.Helpers;

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
