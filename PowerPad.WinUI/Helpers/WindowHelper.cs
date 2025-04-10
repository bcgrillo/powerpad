using Microsoft.UI.Xaml;
using System.Collections.Generic;

namespace PowerPad.WinUI.Helpers
{
    public static class WindowHelper
    {
        public static IReadOnlyList<Window> ActiveWindows { get { return _activeWindows.AsReadOnly(); } }

        private readonly static List<Window> _activeWindows = [];
        private static MainWindow? _mainWindow;

        public static T CreateWindow<T>() where T : Window, new()
        {
            var newWindow = new T();
            TrackWindow(newWindow);

            if (newWindow is MainWindow mainWindow) _mainWindow = mainWindow;

            return newWindow;
        }

        public static void TrackWindow(Window window)
        {
            window.Closed += (s, e) => _activeWindows.Remove(window);

            _activeWindows.Add(window);
        }

        public static Window? GetWindowForElement(UIElement element)
        {
            if (element.XamlRoot is not null)
            {
                foreach (Window window in _activeWindows)
                {
                    if (element.XamlRoot == window.Content.XamlRoot)
                    {
                        return window;
                    }
                }
            }
            return null;
        }

        public static MainWindow MainWindow => _mainWindow!;
    }
}