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
using Azure;
using PowerPad.WinUI.Pages.Providers;

namespace PowerPad.WinUI.Pages
{
    public sealed partial class ModelsPage : Page, INavigationPage
    {
        public double NavigationWidth => NavView.IsPaneVisible ? NavView.OpenPaneLength : 0;

        private readonly SettingsViewModel _services;

        public ModelsPage()
        {
            this.InitializeComponent();

            _services = Ioc.Default.GetRequiredService<SettingsViewModel>();

            NavView.SelectedItem = NavView.MenuItems[0];
            NavFrame.Navigate(typeof(OllamaModelsPage));
        }

        public event EventHandler? NavigationVisibilityChanged;

        private void NavView_SelectionChanged(NavigationView _, NavigationViewSelectionChangedEventArgs args)
        {
            switch ((args.SelectedItem as NavigationViewItem)?.Tag)
            {
                case "Ollama":
                    NavFrame.Navigate(typeof(OllamaModelsPage));
                    break;
                case "HuggingFace":
                    NavFrame.Navigate(typeof(HuggingFaceModelsPage));
                    break;
                case "Github":
                    NavFrame.Navigate(typeof(AzureAIModelsPage));
                    break;
                case "OpenAI":
                    NavFrame.Navigate(typeof(OpenAIModelsPage));
                    break;
                default:
                    break;
            }
        }

        public void ToggleNavigationVisibility()
        {
            NavView.IsPaneVisible = !NavView.IsPaneVisible;

            NavigationVisibilityChanged?.Invoke(this, null!);
        }
    }
}