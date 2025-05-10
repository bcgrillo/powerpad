using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using PowerPad.WinUI.Helpers;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using Windows.UI.WindowManagement;
using WinUIEx;

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

        public void ShowPopup()
        {
            this.CenterOnScreen();
            this.Show();
            this.SetIsAlwaysOnTop(true);
            PopupEditorPage.SetFocus();
        }

        public void SetContent(string newContent) => PopupEditorPage.SetContent(newContent);

        private void PopupEditorPage_CloseRequested(object _, EventArgs __) => Close();

        private void PopupWindow_Closed(object _, WindowEventArgs eventArgs)
        {
            eventArgs.Handled = true;
            this.SetIsAlwaysOnTop(false);
            this.Hide();
        }
    }
}