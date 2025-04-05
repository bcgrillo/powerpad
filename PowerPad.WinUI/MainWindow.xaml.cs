using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using WinUIEx;
using Microsoft.UI.Composition.SystemBackdrops;
using WinRT;
using Microsoft.UI.Composition;
using PowerPad.WinUI.Pages;
using Microsoft.UI.Windowing;
using System.Xml.Linq;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.UI;

namespace PowerPad.WinUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : WindowEx
    {
        private readonly DesktopAcrylicController? _acrylicController;
        private readonly SystemBackdropConfiguration? _configurationSource;

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
            this.InitializeComponent();
            SetTitleBar();

            NavView.SelectedItem = NavView.MenuItems[0];

            if (DesktopAcrylicController.IsSupported())
            {
                _configurationSource = new SystemBackdropConfiguration
                {
                    IsInputActive = true,
                    Theme = (SystemBackdropTheme)((FrameworkElement)Content).ActualTheme
                };

                _acrylicController = new DesktopAcrylicController
                {
                    Kind = DesktopAcrylicKind.Thin,
                    TintColor = (Color)Application.Current.Resources["PowerPadBackGroundColor"],
                    TintOpacity = 0.4F
                };

                _acrylicController.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
                _acrylicController.SetSystemBackdropConfiguration(_configurationSource);
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

            TitleBar.Margin = new Thickness(navWidth, 0, 0, 0);

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
