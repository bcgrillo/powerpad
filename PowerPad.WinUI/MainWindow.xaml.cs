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

namespace PowerPad.WinUI
{
    public partial class MainWindow : WindowEx
    {
        private DesktopAcrylicController? _acrylicController;
        private SystemBackdropConfiguration? _configurationSource;
        private readonly SettingsViewModel _settings;

        private string? _activePageName;
        private INavigationPage? _activeNavPage;

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

            NavView.SelectedItem = NavView.MenuItems[0];

            Closed += (s, e) =>
            {
                App.Get<IConfigStoreService>().StoreConfigs();
                EditorManagerHelper.AutoSaveEditors();
            };
            
            UpdateNavMenuItems();

            _settings.General.PropertyChanged += (s, e) => UpdateNavMenuItems();
            _settings.Models.PropertyChanged += (s, e) => UpdateNavMenuItems();
        }

        private void UpdateNavMenuItems()
        {
            //TODO: Add a better way to handle this
            ModelsNavViewItem.IsEnabled = _settings.General.OllamaEnabled || _settings.General.AzureAIEnabled || _settings.General.OpenAIEnabled;
            AgentesNavViewItem.IsEnabled = _settings.Models.DefaultModel is not null;

            ErrorBadge.Visibility = (_settings.General.OllamaConfig.ServiceStatus == ServiceStatus.Error)
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
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(TitleBar);
            this.AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
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

                _activeNavPage = NavFrame.Content as INavigationPage;

                if (_activeNavPage is not null)
                {
                    _activeNavPage.NavigationVisibilityChanged += NavigationVisibilityChanged;
                    (NavFrame.Content as Page)!.Loaded += (s, e) => NavigationVisibilityChanged(null, null);
                }

                NavigationVisibilityChanged(null, null);
            }
        }

        private void NavigationVisibilityChanged(object? _, EventArgs? __)
        {
            var navWidth = _activeNavPage?.NavigationWidth ?? 0;

            TitleBar.Margin = new(navWidth, 0, 0, 0);

            if (navWidth > 0) Splitter.Visibility = Visibility.Visible;
            else Splitter.Visibility = Visibility.Collapsed;
        }

        private void NavigationViewItem_PointerPressed(object sender, PointerRoutedEventArgs __)
        {
            var navigationViewItem = (NavigationViewItem)sender;

            if (navigationViewItem.Tag.ToString() == _activePageName) (NavFrame.Content as INavigationPage)?.ToggleNavigationVisibility();
        }
    }
}
