using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace PowerPad.WinUI.Helpers
{
    public static class VisualTreeHelperExtensions
    {
        public static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject is null) return null;

            if (parentObject is T parent)
            {
                return parent;
            }
            else
            {
                return FindParent<T>(parentObject);
            }
        }
    }
}