using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WinRT;

namespace PowerPad.WinUI.Helpers
{
    /// <summary>
    /// Provides helper methods to manage and configure system backdrops for the application.
    /// </summary>
    public static class BackdropHelper
    {
        private static DesktopAcrylicController? _acrylicController;
        private static SystemBackdropConfiguration? _configurationSource;

        /// <summary>
        /// Configures and sets the system backdrop for the application window.
        /// </summary>
        /// <param name="setAcrylicBackDrop">Indicates whether to enable the acrylic backdrop.</param>
        /// <param name="appTheme">The application theme to apply to the backdrop. Can be null.</param>
        /// <param name="window">The application window to which the backdrop will be applied.</param>
        /// <param name="mainPage">The main page of the application, used to configure the background.</param>
        public static void SetBackdrop(bool setAcrylicBackDrop, ApplicationTheme? appTheme, Window window, Page mainPage)
        {
            if (setAcrylicBackDrop && DesktopAcrylicController.IsSupported())
            {
                _configurationSource ??= new()
                {
                    IsInputActive = true,
                    Theme = appTheme.HasValue
                        ? (appTheme.Value == ApplicationTheme.Light ? SystemBackdropTheme.Light : SystemBackdropTheme.Dark)
                        : (SystemBackdropTheme)mainPage.ActualTheme
                };

                _acrylicController ??= new()
                {
                    Kind = DesktopAcrylicKind.Thin,
                    TintColor = ((SolidColorBrush)Application.Current.Resources["PowerPadBackgroundBrush"]).Color,
                    TintOpacity = 0.8F
                };

                mainPage.Background = null;
                _acrylicController.AddSystemBackdropTarget(window.As<ICompositionSupportsSystemBackdrop>());
                _acrylicController.SetSystemBackdropConfiguration(_configurationSource);
            }
            else
            {
                mainPage.Background = (Brush)Application.Current.Resources["PowerPadBackgroundBrush"];
            }
        }

        /// <summary>
        /// Disposes the acrylic controller to release resources.
        /// </summary>
        public static void DisposeController()
        {
            _acrylicController?.Dispose();
        }
    }
}