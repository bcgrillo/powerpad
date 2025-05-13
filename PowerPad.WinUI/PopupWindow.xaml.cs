using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using PowerPad.WinUI.Helpers;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using Windows.UI.WindowManagement;
using WinUIEx;

namespace PowerPad.WinUI
{
    /// <summary>
    /// Represents a popup window with custom behavior and settings.
    /// </summary>
    public sealed partial class PopupWindow : WindowEx
    {
        private readonly SettingsViewModel _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="PopupWindow"/> class.
        /// </summary>
        public PopupWindow()
        {
            this.InitializeComponent();

            _settings = App.Get<SettingsViewModel>();

            ExtendsContentIntoTitleBar = true;

            OverlappedPresenter presenter = OverlappedPresenter.Create();
            presenter.IsResizable = false;
            presenter.SetBorderAndTitleBar(true, false);
            AppWindow.SetPresenter(presenter);

            SetTitleBar(PopupEditorPage.TitleBar);

            BackdropHelper.SetBackdrop(_settings.General.AcrylicBackground, _settings.General.AppTheme, this, PopupEditorPage);

            _settings.General.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_settings.General.AcrylicBackground))
                {
                    BackdropHelper.SetBackdrop(_settings.General.AcrylicBackground, _settings.General.AppTheme, this, PopupEditorPage);
                }
            };
        }

        /// <summary>
        /// Displays the popup window, centers it on the screen, and sets it to always be on top.
        /// </summary>
        public void ShowPopup()
        {
            this.CenterOnScreen();
            this.Show();
            this.SetIsAlwaysOnTop(true);
            PopupEditorPage.SetFocus();
        }

        /// <summary>
        /// Sets the content of the popup editor page.
        /// </summary>
        /// <param name="newContent">The new content to display in the popup editor.</param>
        public void SetContent(string newContent) => PopupEditorPage.SetContent(newContent);

        /// <summary>
        /// Handles the close request event for the popup editor page.
        /// </summary>
        private void PopupEditorPage_CloseRequested(object _, EventArgs __) => Close();

        /// <summary>
        /// Handles the closed event for the popup window.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="eventArgs">The event arguments containing details about the window close event.</param>
        private void PopupWindow_Closed(object _, WindowEventArgs eventArgs)
        {
            eventArgs.Handled = true;
            this.SetIsAlwaysOnTop(false);
            this.Hide();
        }
    }
}