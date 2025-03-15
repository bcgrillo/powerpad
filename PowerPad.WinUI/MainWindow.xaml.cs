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

        public MainWindow()
        {
            this.InitializeComponent();
            SetTitleBar();

            NavView.SelectedItem = NavView.MenuItems[0];
            Navigate(typeof(NotesPage));

            if (DesktopAcrylicController.IsSupported())
            {
                _configurationSource = new SystemBackdropConfiguration
                {
                    IsInputActive = true,
                    Theme = (SystemBackdropTheme)((FrameworkElement)Content).ActualTheme
                };

                _acrylicController = new DesktopAcrylicController
                {
                    Kind = DesktopAcrylicKind.Thin
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
            this.AppWindow.SetIcon("Assets/AppIcon/Icon.ico");
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
                switch (selectedItem.Tag)
                {
                    case "Notas":
                        Navigate(typeof(NotesPage));
                        break;
                    case "Modelos":
                        Navigate(typeof(AIServicesPage));
                        break;
                }


            }
        }

        private void Navigate(Type type)
        {
            NavFrame.Navigate(type);

            if (NavFrame.Content is INavigationPage navigationPage)
            {
                navigationPage.NavigationVisibilityChanged += NavigationVisibilityChanged;
            }
        }

        private void NavigationVisibilityChanged(object? sender, NavigationVisibilityChangedEventArgs eventArgs)
        {
            TitleBar.Margin = new Thickness(eventArgs.Width, 0, 0, 0);
        }

        private void NavigationViewItem_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            (NavFrame.Content as INavigationPage)?.ToggleNavigationVisibility();
        }

        private void WindowEx_Closed(object sender, WindowEventArgs args)
        {
            
        }
    }
}
