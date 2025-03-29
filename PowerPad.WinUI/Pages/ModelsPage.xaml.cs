using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.Pages.Providers;
using PowerPad.Core.Models;
using PowerPad.WinUI.ViewModels.Settings;

namespace PowerPad.WinUI.Pages
{
    public sealed partial class ModelsPage : Page, INavigationPage
    {
        public double NavigationWidth => NavView.IsPaneVisible ? NavView.OpenPaneLength : 0;

        private readonly SettingsViewModel _settings;

        public ModelsPage()
        {
            this.InitializeComponent();

            _settings = App.Get<SettingsViewModel>();
        }

        public event EventHandler? NavigationVisibilityChanged;

        private void NavView_SelectionChanged(NavigationView _, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem != null) 
                NavigateToPage((ModelProvider)((NavigationViewItem)args.SelectedItem)!.Tag);
        }

        private void NavigateToPage(ModelProvider modelProvider)
        {
            switch (modelProvider)
            {
                case ModelProvider.Ollama:
                    NavFrame.Navigate(typeof(OllamaModelsPage));
                    break;
                case ModelProvider.HuggingFace:
                    NavFrame.Navigate(typeof(HuggingFaceModelsPage));
                    break;
                case ModelProvider.GitHub:
                    NavFrame.Navigate(typeof(AzureAIModelsPage));
                    break;
                case ModelProvider.OpenAI:
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

        private void NavView_Loaded(object _, RoutedEventArgs __)
        {
            NavView.SelectedItem = NavView.MenuItems
                .FirstOrDefault(mi => ((NavigationViewItem)mi).Visibility == Visibility.Visible);

            if (NavView.SelectedItem != null)
                NavigateToPage((ModelProvider)(NavView.SelectedItem as NavigationViewItem)!.Tag);
        }
    }
}