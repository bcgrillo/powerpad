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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

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
        private INavigationPage? _activePage;

        private readonly Dictionary<string, Type> _navigation = new()
        {
            { nameof(NotesPage), typeof(NotesPage) },
            { nameof(AIServicesPage), typeof(AIServicesPage) }
        };

        public MainWindow()
        {
            this.InitializeComponent();
            SetTitleBar();

            NavView.SelectedItem = NavView.MenuItems[0];
            NavigateToPage(nameof(NotesPage));

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
                    TintOpacity = 0.3F
                };

                _acrylicController.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
                _acrylicController.SetSystemBackdropConfiguration(_configurationSource);
            }
        }

        private void SetTitleBar()
        {
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(TitleBar);
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                // Navigate to settings page
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

            if (_navigation.TryGetValue(page, out Type? value) && typeof(INavigationPage).IsAssignableFrom(value))
            {
                NavFrame.Navigate(value);

                _activePage = NavFrame.Content as INavigationPage;
                _activePageName = page;

                if (_activePage != null)
                {
                    _activePage.NavigationVisibilityChanged += NavigationVisibilityChanged;
                    (NavFrame.Content as Page)!.Loaded += (sender, args) => NavigationVisibilityChanged(null, null!);
                }
            }
        }

        private void NavigationVisibilityChanged(object? sender, EventArgs args)
        {
            TitleBar.Margin = new Thickness(_activePage?.NavigationWidth ?? 0, -8, 0, 0);
        }

        private void NavigationViewItem_PointerPressed(object sender, PointerRoutedEventArgs args)
        {
            var navigationViewItem = (NavigationViewItem)sender;

            if (navigationViewItem.Tag.ToString() == _activePageName) (NavFrame.Content as INavigationPage)?.ToggleNavigationVisibility();
        }

        private void WindowEx_Closed(object sender, WindowEventArgs args)
        {

        }
    }
}
