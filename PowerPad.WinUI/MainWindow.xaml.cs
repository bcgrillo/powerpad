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
using PowerPad.Core.Services.AI;
using PowerPad.WinUI.ViewModels.Settings;
using PowerPad.WinUI.Dialogs;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

namespace PowerPad.WinUI
{
    public partial class MainWindow : WindowEx
    {
        private DesktopAcrylicController? _acrylicController;
        private SystemBackdropConfiguration? _configurationSource;
        private readonly GeneralSettingsViewModel _generalSettings;

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
            _generalSettings = App.Get<SettingsViewModel>().General;

            this.InitializeComponent();
            SetBackdrop();
            SetTitleBar();

            NavView.SelectedItem = NavView.MenuItems[0];
            Closed += (s, e) => EditorManagerHelper.AutoSaveEditors();

            StartOllama();
        }

        private void StartOllama()
        {
            if (_generalSettings.OllamaEnabled && _generalSettings.OllamaAutostart)
            {
                try
                {
                    App.Get<IOllamaService>().Start();
                }
                catch
                {
                    //TODO: Something
                }
            }
        }

        public void SetBackdrop()
        {
            if (_generalSettings.AcrylicBackground && DesktopAcrylicController.IsSupported())
            {
                _configurationSource ??= new()
                {
                    IsInputActive = true,
                    Theme = _generalSettings.AppTheme is null
                        ? (SystemBackdropTheme)((FrameworkElement)Content).ActualTheme
                        : (_generalSettings.AppTheme == ApplicationTheme.Light ? SystemBackdropTheme.Light : SystemBackdropTheme.Dark)
                };

                _acrylicController ??= new()
                {
                    Kind = DesktopAcrylicKind.Thin,
                    TintColor = (Color)Application.Current.Resources["PowerPadBackGroundColor"],
                    TintOpacity = 0.4F
                };

                MainPage.Background = null;
                _acrylicController.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
                _acrylicController.SetSystemBackdropConfiguration(_configurationSource);
            }
            else
            {
                MainPage.Background = new SolidColorBrush((Color)Application.Current.Resources["PowerPadBackGroundColor"]);
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
