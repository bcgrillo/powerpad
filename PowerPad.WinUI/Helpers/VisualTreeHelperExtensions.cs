using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace PowerPad.WinUI.Helpers
{
    /// <summary>
    /// Provides extension methods for working with the visual tree in WinUI applications.
    /// </summary>
    public static class VisualTreeHelperExtensions
    {
        /// <summary>
        /// Finds the first parent of a specified type in the visual tree for a given child element.
        /// </summary>
        /// <typeparam name="T">The type of the parent to find.</typeparam>
        /// <param name="child">The child element for which to find the parent.</param>
        /// <returns>The first parent of the specified type, or null if no such parent exists.</returns>
        public static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            // Get the parent of the current child element.
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject is null) return null;

            // Check if the parent is of the specified type.
            if (parentObject is T parent)
            {
                return parent;
            }
            else
            {
                // Recursively search for the parent in the visual tree.
                return FindParent<T>(parentObject);
            }
        }
    }
}