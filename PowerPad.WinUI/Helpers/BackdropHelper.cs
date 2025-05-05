using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinRT;

namespace PowerPad.WinUI.Helpers
{
    public static class BackdropHelper
    {
        private static DesktopAcrylicController? _acrylicController;
        private static SystemBackdropConfiguration? _configurationSource;

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

        public static void DisposeController()
        {
            _acrylicController?.Dispose();
        }
    }
}