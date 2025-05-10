using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Models.AI;
using PowerPad.WinUI.Pages.Providers;
using PowerPad.WinUI.ViewModels.AI;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Linq;

namespace PowerPad.WinUI.Pages
{
    public partial class ModelsPage : DisposablePage, IToggleMenuPage
    {
        private IModelProviderPage? _currentPage;
        private bool _runSearch;

        public double NavigationWidth => NavView.IsPaneVisible ? NavView.OpenPaneLength : 0;

        private readonly SettingsViewModel _settings;

        public ModelsPage()
        {
            this.InitializeComponent();

            _settings = App.Get<SettingsViewModel>();
        }

        private void NavView_SelectionChanged(NavigationView __, NavigationViewSelectionChangedEventArgs eventArgs)
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
                    NavView.SelectedItem = selectedItem.MenuItems.First();
                });
            }
        }

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
            else //MenuOption.AddModels
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

        public void ToggleNavigationVisibility()
        {
            NavView.IsPaneVisible = !NavView.IsPaneVisible;
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _currentPage?.Dispose();
                if (_currentPage is AIModelsPageBase currentAIModelsPage) currentAIModelsPage.AddButtonClick -= AddButtonClick;
            }
        }

        private void NavView_PointerPressed(object _, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs __)
        {
            _currentPage?.CloseModelInfoViewer();
        }
    }
}