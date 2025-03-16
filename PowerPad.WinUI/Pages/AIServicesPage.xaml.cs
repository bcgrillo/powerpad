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

namespace PowerPad.WinUI.Pages
{
    public sealed partial class AIServicesPage : Page, INavigationPage
    {
        public double NavigationWidth => NavView.Visibility == Visibility.Visible ? NavView.ActualWidth : 0;

        public ObservableCollection<AIServiceViewModel> Services { get; set; }

        public AIServicesPage()
        {
            this.InitializeComponent();

            Services = [];
            
            Services.Add(new OllamaViewModel(Ioc.Default.GetRequiredService<IOllamaService>()));
        }

        public event EventHandler? NavigationVisibilityChanged;

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {

        }

        public void ToggleNavigationVisibility()
        {
            NavView.Visibility = NavView.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

            NavigationVisibilityChanged?.Invoke(this, null!);
        }
    }
}