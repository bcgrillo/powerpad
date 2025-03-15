using PowerPad.WinUI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Pages
{
    public interface INavigationPage
    {
        void ToggleNavigationVisibility();

        public event EventHandler<NavigationVisibilityChangedEventArgs>? NavigationVisibilityChanged;
    }

    public class NavigationVisibilityChangedEventArgs(double width) : EventArgs 
    {
        public double Width { get; set; } = width;
    }
}
