using Microsoft.UI.Windowing;
using Microsoft.UI;
using PowerPad.WinUI.Helpers;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using Windows.Graphics;
using WinRT.Interop;
using WinUIEx;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace PowerPad.WinUI
{
    public sealed partial class PopupWindow : WindowEx
    {
        private readonly SettingsViewModel _settings;

        public PopupWindow()
        {
            this.InitializeComponent();

            _settings = App.Get<SettingsViewModel>();

            ExtendsContentIntoTitleBar = true;
            BackdropHelper.SetBackdrop(_settings.General.AcrylicBackground, _settings.General.AppTheme, this, PopupEditorPage);

            Closed += (s, e) =>
            {
                e.Handled = true;
                this.SetIsAlwaysOnTop(false);
                this.Hide();
            };
        }

        public void Invoke()
        {
            this.CenterOnScreen();
            this.Show();
            this.SetIsAlwaysOnTop(true);
        }

        public void SetContent(string newContent) => PopupEditorPage.SetContent(newContent);

        //Visual tree may not have been loaded yet at Ctor. 
        //It's suggested registering Loaded event of the root element of the window.
        private void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            //Gets the close button element. Refer to the Visual Tree.
            var contentPresenter = VisualTreeHelper.GetParent(this.Content);
            var layoutRoot = VisualTreeHelper.GetParent(contentPresenter);
            var titleBar = VisualTreeHelper.GetChild(layoutRoot, 1) as Grid;
            var buttonContainer = VisualTreeHelper.GetChild(titleBar, 0) as Grid;
            buttonContainer.Visibility = Visibility.Collapsed;
        }
    }
}