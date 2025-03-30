using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.Pages.Providers;
using PowerPad.WinUI.ViewModels.Settings;
using PowerPad.Core.Models.AI;
using PowerPad.WinUI.ViewModels.AI;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls.Primitives;
using PowerPad.WinUI.Helpers;
using System.Threading.Tasks;
using Windows.ApplicationModel.Payments;

namespace PowerPad.WinUI.Pages
{
    public sealed partial class ModelsPage : Page, INavigationPage
    {
        private AIModelsPageBase? _currentAIModelsPage;

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
            var selectedItem = (NavigationViewItem)args.SelectedItem;

            var modelTag = selectedItem.Tag as ModelProvider?;
            var optionTag = selectedItem.Tag as ModelsMenuOption?;

            if (optionTag.HasValue)
            {
                switch (optionTag.Value.Option)
                {
                    case MenuOption.AvailableModels:
                        NavigateToPage(optionTag.Value.ModelProvider);
                        break;
                    case MenuOption.AddModels:
                        break;
                }
            }
            else if (modelTag != null)
            {
                foreach(NavigationViewItem item in NavView.MenuItems)
                    if (item != selectedItem) item.IsExpanded = false;

                NavView.SelectedItem = selectedItem.MenuItems.First();
            }
        }

        private void NavigateToPage(ModelProvider modelProvider)
        {
            _currentAIModelsPage?.Dispose();

            switch (modelProvider)
            {
                case ModelProvider.Ollama:
                    NavFrame.Navigate(typeof(OllamaModelsPage));
                    _currentAIModelsPage = (AIModelsPageBase)NavFrame.Content;
                    break;
                case ModelProvider.HuggingFace:
                    NavFrame.Navigate(typeof(HuggingFaceModelsPage));
                    _currentAIModelsPage = (AIModelsPageBase)NavFrame.Content;
                    break;
                case ModelProvider.GitHub:
                    NavFrame.Navigate(typeof(GitHubModelsPage));
                    _currentAIModelsPage = (AIModelsPageBase)NavFrame.Content;
                    break;
                case ModelProvider.OpenAI:
                    NavFrame.Navigate(typeof(OpenAIModelsPage));
                    _currentAIModelsPage = (AIModelsPageBase)NavFrame.Content;
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
            var firstItem = NavView.MenuItems.FirstOrDefault(mi => ((NavigationViewItem)mi).Visibility == Visibility.Visible) as NavigationViewItem;

            if (firstItem != null)
            {
                firstItem.IsExpanded = true;
                NavView.SelectedItem = firstItem.MenuItems.FirstOrDefault();
            }
        }
    }
}