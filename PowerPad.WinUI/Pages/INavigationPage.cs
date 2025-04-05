using System;

namespace PowerPad.WinUI.Pages
{
    public interface INavigationPage
    {
        double NavigationWidth { get; }

        public event EventHandler NavigationVisibilityChanged;

        void ToggleNavigationVisibility();
    }
}
