using H.NotifyIcon;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Models.AI;
using PowerPad.Core.Services.Config;
using PowerPad.WinUI.Dialogs;
using PowerPad.WinUI.Helpers;
using PowerPad.WinUI.Pages;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Collections.Generic;
using WinUIEx;

namespace PowerPad.WinUI
{
    /// <summary>
    /// Represents the main window of the application, providing navigation and UI management.
    /// </summary>
    public partial class MainWindow : WindowEx
    {
        private readonly SettingsViewModel _settings;
        private readonly Dictionary<string, Type> _navigation = new()
        {
            { nameof(WorkspacePage), typeof(WorkspacePage) },
            { nameof(ModelsPage), typeof(ModelsPage) },
            { nameof(SettingsPage), typeof(SettingsPage) },
            { nameof(AgentsPage), typeof(AgentsPage) },
        };

        private string? _activePageName;
        private IToggleMenuPage? _activeToggleMenuPage;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
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

            // Register HotKey
            if (_settings.General.EnableHotKeys) HotKeyHelper.Register(this, true);
        }


        /// <summary>
        /// Sets the backdrop effect for the application window.
        /// </summary>
        /// <param name="value">A value indicating whether to enable the backdrop effect.</param>
        public void SetBackdrop(bool value)
        {
            BackdropHelper.SetBackdrop(value, _settings.General.AppTheme, this, MainPage);
        }

        /// <summary>
        /// Displays the notes page in the application.
        /// </summary>
        public void ShowNotes()
        {
            this.Show();
            BringToFront();
            NavView.SelectedItem = NavView.MenuItems[0];
        }

        /// <summary>
        /// Handles the Loaded event of the NavigationView control.
        /// </summary>
        private void NavView_Loaded(object _, RoutedEventArgs __)
        {
            DispatcherQueue.TryEnqueue(async () =>
            {
                NavView.SelectedItem = NavView.MenuItems[0];

                _settings.General.OllamaConfig.StatusChanged += (s, e) => UpdateNavMenuItems();
                _settings.General.AzureAIConfig.StatusChanged += (s, e) => UpdateNavMenuItems();
                _settings.General.OpenAIConfig.StatusChanged += (s, e) => UpdateNavMenuItems();
                _settings.Models.ModelAvailabilityChanged += (s, e) => UpdateNavMenuItems();

                await _settings.TestConnections();

                if (_settings.General.OllamaEnabled && _settings.General.OllamaConfig.ServiceStatus == ServiceStatus.NotFound)
                {
                    var ollamaInstallationDialog = await OllamaDownloadHelper.ShowAsync(Content.XamlRoot);

                    switch (ollamaInstallationDialog)
                    {
                        case ContentDialogResult.Primary:
                            NavView.SelectedItem = NavView.MenuItems[1]; // Go to models
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

        /// <summary>
        /// Updates the navigation menu items based on the current settings.
        /// </summary>
        private void UpdateNavMenuItems()
        {
            ModelsNavViewItem.IsEnabled = _settings.General.OllamaEnabled || _settings.General.AzureAIEnabled || _settings.General.OpenAIEnabled;
            AgentesNavViewItem.IsEnabled = _settings.IsAIAvailable == true;

            ErrorBadge.Visibility = (_settings.General.OllamaConfig.ServiceStatus == ServiceStatus.Error)
                || (_settings.General.OllamaConfig.ServiceStatus == ServiceStatus.NotFound)
                || (_settings.General.AzureAIConfig.ServiceStatus == ServiceStatus.Error)
                || (_settings.General.OpenAIConfig.ServiceStatus == ServiceStatus.Error)
                ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Configures the title bar of the application window.
        /// </summary>
        private void SetTitleBar()
        {
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBar);
            AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        }

        /// <summary>
        /// Handles the SelectionChanged event of the NavigationView control.
        /// </summary>
        /// <param name="_">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments containing selection details.</param>
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

        /// <summary>
        /// Navigates to the specified page.
        /// </summary>
        /// <param name="page">The name of the page to navigate to.</param>
        private void NavigateToPage(string? page)
        {
            ArgumentException.ThrowIfNullOrEmpty(page);

            if (_activePageName != page)
            {
                _activePageName = page;

                NavFrame.Navigate(_navigation[page]);

                _activeToggleMenuPage = NavFrame.Content as IToggleMenuPage;

                (NavFrame.Content as Page)!.Loaded += (s, e) => SetToggleVisualElements();
            }
        }

        /// <summary>
        /// Sets the visual elements for toggling the navigation menu.
        /// </summary>
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
                    ToggleMenuIcon.Glyph = "\uE8A0";
                }
                else
                {
                    Splitter.Visibility = Visibility.Collapsed;
                    ToggleMenuIcon.Glyph = "\uE89F";
                }

                ToggleMenuBtn.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Handles the click event of the ToggleMenu button.
        /// </summary>
        private void ToggleMenuBtn_Click(object _, RoutedEventArgs __)
        {
            (NavFrame.Content as IToggleMenuPage)?.ToggleNavigationVisibility();

            SetToggleVisualElements();
        }

        /// <summary>
        /// Minimizes the application window to the system tray.
        /// </summary>
        private void MinimizeToTray_Click(object _, RoutedEventArgs __)
        {
            TaskbarIcon.Visibility = Visibility.Visible;
            this.Hide(enableEfficiencyMode: false);
        }

        /// <summary>
        /// Restores the application window from the system tray.
        /// </summary>
        private void Show_Click(object _, RoutedEventArgs __)
        {
            TaskbarIcon.Visibility = Visibility.Collapsed;
            this.Show();
        }

        /// <summary>
        /// Exits the application.
        /// </summary>
        private void Exit_Click(object _, RoutedEventArgs __)
        {
            Application.Current.Exit();
        }
    }
}
