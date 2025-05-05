using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUIEx;
using Microsoft.UI.Composition.SystemBackdrops;
using WinRT;
using Microsoft.UI.Composition;
using PowerPad.WinUI.Pages;
using Microsoft.UI.Windowing;
using PowerPad.WinUI.Helpers;
using PowerPad.WinUI.ViewModels.Settings;
using Microsoft.UI.Xaml.Media;
using PowerPad.Core.Services.Config;
using PowerPad.Core.Models.AI;
using PowerPad.WinUI.Dialogs;
using H.NotifyIcon;

namespace PowerPad.WinUI
{
    public partial class MainWindow : WindowEx
    {
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
            this.InitializeComponent();

            _settings = App.Get<SettingsViewModel>();

            SetBackdrop(_settings.General.AcrylicBackground);
            SetTitleBar();

            Closed += (s, e) =>
            {
                App.Get<IConfigStoreService>().StoreConfigs();
                EditorManagerHelper.AutoSaveEditors();

                BackdropHelper.DisposeController();
            };

            // Registrar HotKey
            HotKeyHelper.Register(this);
        }

        private void NavView_Loaded(object _, RoutedEventArgs __)
        {
            DispatcherQueue.TryEnqueue(async () =>
            {
                NavView.SelectedItem = NavView.MenuItems[0];

                _settings.General.OllamaConfig.StatusChanged += (s, e) => UpdateNavMenuItems();
                _settings.General.AzureAIConfig.StatusChanged += (s, e) => UpdateNavMenuItems();
                _settings.General.OpenAIConfig.StatusChanged += (s, e) => UpdateNavMenuItems();
                _settings.Models.ModelAvaibilityChanged += (s, e) => UpdateNavMenuItems();

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

        private void SetTitleBar()
        {
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBar);
            AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        }

        private void NavView_SelectionChanged(object _, NavigationViewSelectionChangedEventArgs eventArgs)
        {
            if (eventArgs.IsSettingsSelected)
            {
                NavigateToPage(nameof(SettingsPage));
            }
            else
            {
                var selectedItem = (NavigationViewItem)eventArgs.SelectedItem;

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
                TitleContent.Margin = TitleContent.Margin with { Left = 0 };
                TitleBar.Margin = new(0);
                Splitter.Visibility = Visibility.Collapsed;
                ToggleMenuBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                var navWidth = _activeToggleMenuPage.NavigationWidth;

                TitleContent.Margin = TitleContent.Margin with { Left = navWidth };
                TitleBar.Margin = new(8, 0, 0, 0);

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

        private void MinimizeToTray_Click(object _, RoutedEventArgs __)
        {
            TaskbarIcon.Visibility = Visibility.Visible;
            this.Hide(enableEfficiencyMode: false);
        }

        private void Show_Click(object _, RoutedEventArgs __)
        {
            TaskbarIcon.Visibility = Visibility.Collapsed;
            this.Show();
        }

        private void Exit_Click(object _, RoutedEventArgs __)
        {
            Application.Current.Exit();
        }

        public void SetBackdrop(bool value)
        {
            BackdropHelper.SetBackdrop(value, _settings.General.AppTheme, this, MainPage);
        }
    }
}
