using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.WinUI.ViewModels;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using PowerPad.Core.Services;
using PowerPad.WinUI.Components;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PowerPad.WinUI.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AIServicesPage : Page, INavigationPage
    {
        public ObservableCollection<AIServiceViewModel> Services { get; set; }

        public AIServicesPage()
        {
            this.InitializeComponent();

            Services = [];
            
            Services.Add(new OllamaViewModel(Ioc.Default.GetRequiredService<IOllamaService>()));
        }

        public event EventHandler<NavigationVisibilityChangedEventArgs>? NavigationVisibilityChanged;

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {

        }

        public void ToggleNavigationVisibility()
        {
            var isVisible = NavView.Visibility == Visibility.Visible;

            NavView.Visibility = isVisible ? Visibility.Collapsed : Visibility.Visible;

            isVisible = !isVisible;

            NavigationVisibilityChanged?.Invoke(this, new NavigationVisibilityChangedEventArgs(isVisible ? NavView.ActualWidth : 0));
        }
    }
}
