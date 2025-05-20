using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using PowerPad.Core.Models.AI;
using PowerPad.WinUI.Pages.Providers;
using PowerPad.WinUI.ViewModels.AI;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Linq;

namespace PowerPad.WinUI.Pages
{
    /// <summary>
    /// Represents the page for managing AI models and navigation between different model-related pages.
    /// </summary>
    public partial class ModelsPage : DisposablePage, IToggleMenuPage
    {
        private readonly SettingsViewModel _settings;

        private IModelProviderPage? _currentPage;
        private bool _runSearch;

        /// <summary>
        /// Gets the width of the navigation pane.
        /// </summary>
        public double NavigationWidth => NavView.IsPaneVisible ? NavView.OpenPaneLength : 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelsPage"/> class.
        /// </summary>
        public ModelsPage()
        {
            this.InitializeComponent();
            _settings = App.Get<SettingsViewModel>();
        }

        /// <summary>
        /// Toggles the visibility of the navigation pane.
        /// </summary>
        public void ToggleNavigationVisibility()
        {
            NavView.IsPaneVisible = !NavView.IsPaneVisible;
        }

        /// <summary>
        /// Handles the selection change event in the navigation view.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="eventArgs">The event arguments containing the selected item.</param>
        private void NavView_SelectionChanged(NavigationView _, NavigationViewSelectionChangedEventArgs eventArgs)
        {
            if (eventArgs.SelectedItem is null) return;

            var selectedItem = (NavigationViewItem)eventArgs.SelectedItem;

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
                    NavView.SelectedItem = selectedItem.MenuItems[0];
                });
            }
        }

        /// <summary>
        /// Navigates to the specified page based on the provided menu option.
        /// </summary>
        /// <param name="menuOption">The menu option specifying the target page and model provider.</param>
        private void NavigateToPage(ModelsMenuOption menuOption)
        {
            _currentPage?.Dispose();
            if (_currentPage is AIModelsPageBase currentAIModelsPage) currentAIModelsPage.AddButtonClick -= AddButtonClick;

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

                ((AIModelsPageBase)_currentPage!).AddButtonClick += AddButtonClick;
            }
            else // MenuOption.AddModels
            {
                switch (menuOption.ModelProvider)
                {
                    case ModelProvider.Ollama:
                        NavFrame.Navigate(typeof(OllamaAddModelPage));
                        _currentPage = (OllamaAddModelPage)NavFrame.Content;
                        break;
                    case ModelProvider.HuggingFace:
                        NavFrame.Navigate(typeof(HuggingFaceAddModelPage));
                        _currentPage = (HuggingFaceAddModelPage)NavFrame.Content;
                        break;
                    case ModelProvider.GitHub:
                        NavFrame.Navigate(typeof(GitHubAddModelPage));
                        _currentPage = (GitHubAddModelPage)NavFrame.Content;
                        break;
                    case ModelProvider.OpenAI:
                        NavFrame.Navigate(typeof(OpenAIAddModelPage));
                        _currentPage = (OpenAIAddModelPage)NavFrame.Content;
                        break;
                }

                if (_runSearch)
                {
                    ((AIAddModelPageBase)_currentPage!).Search();
                    _runSearch = false;
                }
            }
        }

        /// <summary>
        /// Handles the loaded event of the navigation view.
        /// </summary>
        private void NavView_Loaded(object _, RoutedEventArgs __)
        {
            var firstItem = NavView.MenuItems.FirstOrDefault(mi => ((NavigationViewItem)mi).Visibility == Visibility.Visible) as NavigationViewItem;

            if (firstItem is not null)
            {
                firstItem.IsExpanded = true;
                NavView.SelectedItem = firstItem.MenuItems[0];
            }
        }

        /// <summary>
        /// Handles the Add button click event to navigate to the Add Models page.
        /// </summary>
        private void AddButtonClick(object? _, EventArgs __)
        {
            ModelsMenuOption? modelsMenuOption = null;

            if (_currentPage is OllamaModelsPage) modelsMenuOption = new(ModelProvider.Ollama, MenuOption.AddModels);
            else if (_currentPage is HuggingFaceModelsPage) modelsMenuOption = new(ModelProvider.HuggingFace, MenuOption.AddModels);
            else if (_currentPage is GitHubModelsPage) modelsMenuOption = new(ModelProvider.GitHub, MenuOption.AddModels);
            else if (_currentPage is OpenAIModelsPage) modelsMenuOption = new(ModelProvider.OpenAI, MenuOption.AddModels);

            if (modelsMenuOption.HasValue)
            {
                NavigationViewItem? menuItem;

                foreach (var providerItem in NavView.MenuItems.Cast<NavigationViewItem>())
                {
                    menuItem = providerItem.MenuItems.FirstOrDefault(mi => ((NavigationViewItem)mi).Tag as ModelsMenuOption? == modelsMenuOption) as NavigationViewItem;

                    if (menuItem is not null)
                    {
                        _runSearch = true;
                        NavView.SelectedItem = menuItem;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the pointer pressed event to close the model information viewer.
        /// </summary>
        private void NavView_PointerPressed(object _, PointerRoutedEventArgs __)
        {
            _currentPage?.CloseModelInfoViewer();
        }

        /// <summary>
        /// Disposes resources used by the page.
        /// </summary>
        /// <param name="disposing">Indicates whether the method is called from Dispose.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _currentPage?.Dispose();
                if (_currentPage is AIModelsPageBase currentAIModelsPage) currentAIModelsPage.AddButtonClick -= AddButtonClick;
            }
        }
    }
}