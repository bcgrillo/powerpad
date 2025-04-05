using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.Pages.Providers;
using PowerPad.WinUI.ViewModels.Settings;
using PowerPad.Core.Models.AI;
using PowerPad.WinUI.ViewModels.AI;

namespace PowerPad.WinUI.Pages
{
    public sealed partial class ModelsPage : Page, INavigationPage
    {
        private IDisposable? _currentPage;

        public double NavigationWidth => NavView.IsPaneVisible ? NavView.OpenPaneLength : 0;

        private readonly SettingsViewModel _settings;

        public ModelsPage()
        {
            this.InitializeComponent();

            _settings = App.Get<SettingsViewModel>();
        }

        public event EventHandler? NavigationVisibilityChanged;

        private void NavView_SelectionChanged(NavigationView __, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is null) return;

            var selectedItem = (NavigationViewItem)args.SelectedItem;

            var modelTag = selectedItem.Tag as ModelProvider?;
            var optionTag = selectedItem.Tag as ModelsMenuOption?;

            if (optionTag.HasValue)
            {
                NavigateToPage(optionTag.Value);
            }
            else if (modelTag.HasValue)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    
                    foreach (var item in NavView.MenuItems)
                    {
                        var navItem = (NavigationViewItem)item;

                        if (navItem != selectedItem && navItem.IsExpanded) navItem.IsExpanded = false;
                    }

                    selectedItem.IsExpanded = true;
                    NavView.SelectedItem = selectedItem.MenuItems.First();
                });
            }
        }

        private void NavigateToPage(ModelsMenuOption menuOption)
        {
            _currentPage?.Dispose();

            if (menuOption.Option == MenuOption.AvailableModels)
            { 
                switch (menuOption.ModelProvider)
                {
                    case ModelProvider.Ollama:
                        NavFrame.Navigate(typeof(OllamaModelsPage));
                        _currentPage = (AIModelsPageBase)NavFrame.Content;
                        break;
                    case ModelProvider.HuggingFace:
                        NavFrame.Navigate(typeof(HuggingFaceModelsPage));
                        _currentPage = (AIModelsPageBase)NavFrame.Content;
                        break;
                    case ModelProvider.GitHub:
                        NavFrame.Navigate(typeof(GitHubModelsPage));
                        _currentPage = (AIModelsPageBase)NavFrame.Content;
                        break;
                    case ModelProvider.OpenAI:
                        NavFrame.Navigate(typeof(OpenAIModelsPage));
                        _currentPage = (AIModelsPageBase)NavFrame.Content;
                        break;
                }
            }
            else
            {
                switch (menuOption.ModelProvider)
                {
                    case ModelProvider.Ollama:
                        NavFrame.Navigate(typeof(OllamaAddModelPage));
                        _currentPage = (OllamaAddModelPage)NavFrame.Content;
                        break;
                    case ModelProvider.HuggingFace:
                        break;
                    case ModelProvider.GitHub:
                        break;
                    case ModelProvider.OpenAI:
                        break;
                }
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

            if (firstItem is not null)
            {
                firstItem.IsExpanded = true;
                NavView.SelectedItem = firstItem.MenuItems.First();
            }
        }

        private void HideMenuBtn_Click(object _, RoutedEventArgs __) => ToggleNavigationVisibility();
    }
}