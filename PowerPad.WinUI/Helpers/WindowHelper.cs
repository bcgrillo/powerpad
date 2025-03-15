using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using WinRT.Interop;

namespace PowerPad.WinUI.Helpers
{
    public class WindowHelper
    {
        static public IReadOnlyList<Window> ActiveWindows { get { return _activeWindows.AsReadOnly(); } }

        static private List<Window> _activeWindows = new List<Window>();

        static public T CreateWindow<T>() where T : Window, new()
        {
            T newWindow = new T();
            TrackWindow(newWindow);
            return newWindow;
        }

        static public void TrackWindow(Window window)
        {
            window.Closed += (s, e) => _activeWindows.Remove(window);

            _activeWindows.Add(window);
        }

        static public Window? GetWindowForElement(UIElement element)
        {
            if (element.XamlRoot != null)
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
    }
}