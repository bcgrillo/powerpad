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
        double NavigationWidth { get; }

        public event EventHandler NavigationVisibilityChanged;

        void ToggleNavigationVisibility();
    }
}
