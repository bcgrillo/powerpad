using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using WinUIEx;
using Microsoft.UI.Composition.SystemBackdrops;
using WinRT;
using Microsoft.UI.Composition;
using PowerPad.WinUI.Pages;
using Microsoft.UI.Windowing;
using Windows.UI;
using PowerPad.WinUI.Helpers;
using PowerPad.WinUI.ViewModels.Settings;
using Microsoft.UI.Xaml.Media;
using PowerPad.Core.Services.Config;
using PowerPad.Core.Models.AI;
using PowerPad.WinUI.Dialogs;

namespace PowerPad.WinUI
{
    public partial class MainWindow : WindowEx
    {
        private DesktopAcrylicController? _acrylicController;
        private SystemBackdropConfiguration? _configurationSource;
        private readonly SettingsViewModel _settings;

        private string? _activePageName;
        private IToggleMenuPage? _activeToggleMenuPage;

        private readonly Dictionary<string, Type> _navigation = new()
        {
            { nameof(WorkspacePage), typeof(WorkspacePage) },
            { nameof(ModelsPage), typeof(ModelsPage) },
            { nameof(SettingsPage), typeof(SettingsPage) },
            { nameof(AgentsPage), typeof(AgentsPage) },
        };

        public MainWindow()
        {
            _settings = App.Get<SettingsViewModel>();

            this.InitializeComponent();
            SetBackdrop(_settings.General.AcrylicBackground);
            SetTitleBar();

            Closed += (s, e) =>
            {
                App.Get<IConfigStoreService>().StoreConfigs();
                EditorManagerHelper.AutoSaveEditors();

                _acrylicController?.Dispose();
            };

            _settings.General.OllamaConfig.StatusChanged += (s, e) => UpdateNavMenuItems();
            _settings.General.AzureAIConfig.StatusChanged += (s, e) => UpdateNavMenuItems();
            _settings.General.OpenAIConfig.StatusChanged += (s, e) => UpdateNavMenuItems();
            _settings.Models.ModelAvaibilityChanged += (s, e) => UpdateNavMenuItems();

            NavView.SelectedItem = NavView.MenuItems[0];
        }

        private void MainPage_Loaded(object _, RoutedEventArgs __)
        {
            DispatcherQueue.TryEnqueue(async () =>
            {
                await _settings.TestConnections();

                if(_settings.General.OllamaEnabled && _settings.General.OllamaConfig.ServiceStatus == ServiceStatus.NotFound)
                {
                    var ollamaInstallationDialog = await OllamaDownloadHelper.ShowAsync(Content.XamlRoot);

                    switch (ollamaInstallationDialog)
                    {
                        case ContentDialogResult.Primary:
                            NavView.SelectedItem = NavView.MenuItems[1]; //Go to models
                            break;
                        case ContentDialogResult.Secondary:
                            NavView.SelectedItem = NavView.SettingsItem;
                            break;
                        default:
                            _settings.General.OllamaEnabled = false;
                            break;
                    }
                }

                UpdateNavMenuItems();
            });
        }

        private void UpdateNavMenuItems()
        {
            //TODO: Add a better way to handle this
            ModelsNavViewItem.IsEnabled = _settings.General.OllamaEnabled || _settings.General.AzureAIEnabled || _settings.General.OpenAIEnabled;
            AgentesNavViewItem.IsEnabled = _settings.IsAIAvailable == true;

            ErrorBadge.Visibility = (_settings.General.OllamaConfig.ServiceStatus == ServiceStatus.Error)
                || (_settings.General.OllamaConfig.ServiceStatus == ServiceStatus.NotFound) 
                || (_settings.General.AzureAIConfig.ServiceStatus == ServiceStatus.Error)
                || (_settings.General.OpenAIConfig.ServiceStatus == ServiceStatus.Error)
                ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SetBackdrop(bool setAcrylicBackDrop)
        {
            if (setAcrylicBackDrop && DesktopAcrylicController.IsSupported())
            {
                _configurationSource ??= new()
                {
                    IsInputActive = true,
                    Theme = _settings.General.AppTheme is null
                        ? (SystemBackdropTheme)((FrameworkElement)Content).ActualTheme
                        : (_settings.General.AppTheme == ApplicationTheme.Light ? SystemBackdropTheme.Light : SystemBackdropTheme.Dark)
                };

                _acrylicController ??= new()
                {
                    Kind = DesktopAcrylicKind.Thin,
                    TintColor = (Color)Application.Current.Resources["PowerPadBackgroundColor"],
                    TintOpacity = 0.8F
                };

                MainPage.Background = null;
                _acrylicController.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
                _acrylicController.SetSystemBackdropConfiguration(_configurationSource);
            }
            else
            {
                MainPage.Background = new SolidColorBrush((Color)Application.Current.Resources["PowerPadBackgroundColor"]);
            }
        }

        private void SetTitleBar()
        {
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBar);
            AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        }

        private void NavView_SelectionChanged(object _, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                NavigateToPage(nameof(SettingsPage));
            }
            else
            {
                var selectedItem = (NavigationViewItem)args.SelectedItem;

                NavigateToPage(selectedItem.Tag?.ToString());
            }
        }

        private void NavigateToPage(string? page)
        {
            ArgumentException.ThrowIfNullOrEmpty(page, nameof(page));

            if (_activePageName != page)
            {
                _activePageName = page;

                NavFrame.Navigate(_navigation[page]);

                _activeToggleMenuPage = NavFrame.Content as IToggleMenuPage;

                (NavFrame.Content as Page)!.Loaded += (s, e) => SetToggleVisualElements();
            }
        }

        private void SetToggleVisualElements()
        {
            if (_activeToggleMenuPage is null)
            {
                TitleContent.Margin = new(0);
                Splitter.Visibility = Visibility.Collapsed;
                ToggleMenuBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                var navWidth = _activeToggleMenuPage.NavigationWidth;

                TitleContent.Margin = TitleContent.Margin with { Left = navWidth };

                if (navWidth > 0)
                {
                    Splitter.Visibility = Visibility.Visible;
                    ToggleMenuIcon.Source = (ImageSource)Application.Current.Resources["HideMenuSvg"];
                }
                else
                {
                    Splitter.Visibility = Visibility.Collapsed;
                    ToggleMenuIcon.Source = (ImageSource)Application.Current.Resources["ShowMenuSvg"];
                }

                ToggleMenuBtn.Visibility = Visibility.Visible;
            }

        }

        private void ToggleMenuBtn_Click(object _, RoutedEventArgs __)
        {
            (NavFrame.Content as IToggleMenuPage)?.ToggleNavigationVisibility();

            SetToggleVisualElements();
        }
    }
}
