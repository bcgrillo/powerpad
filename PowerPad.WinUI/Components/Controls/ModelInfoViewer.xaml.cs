using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.WebUI;

namespace PowerPad.WinUI.Components.Controls
{
    public sealed partial class ModelInfoViewer : UserControl
    {
        public event EventHandler<ModelInfoViewerVisibilityEventArgs>? VisibilityChanged;

        public ModelInfoViewer()
        {
            this.InitializeComponent();

            RegisterPropertyChangedCallback(VisibilityProperty, OnVisibilityChanged);

            WebView.CoreWebView2Initialized += WebView_CoreWebView2Initialized;
        }

        private void WebView_CoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
        {
            WebView.CoreWebView2.PermissionRequested += WebView_PermissionRequested;
        }

        public void Show(string title, string url)
        {
            TitleTextBlock.Text = title;
            Visibility = Visibility.Visible;

            if (WebView.CoreWebView2 is not null) WebView.CoreWebView2.NavigationStarting -= WebView_NavigationStarting;

            WebView.Source = new Uri(url);
        }

        public void Hide()
        {
            if (Visibility != Visibility.Collapsed)
            {
                Visibility = Visibility.Collapsed;
                LoadingSpinner.Visibility = Visibility.Visible;
                WebView.Visibility = Visibility.Collapsed;
            }
        }

        private void CloseButton_Click(object _, RoutedEventArgs __) => Hide();

        private void WebView_NavigationCompleted(WebView2 _, CoreWebView2NavigationCompletedEventArgs __)
        {
            WebView.Opacity = 0;
            LoadingSpinner.Visibility = Visibility.Collapsed;
            WebView.Visibility = Visibility.Visible;
            Task.Delay(100).Wait();
            WebView.Opacity = 1;

            WebView.CoreWebView2.NavigationStarting += WebView_NavigationStarting;
        }

        private void OnVisibilityChanged(DependencyObject _, DependencyProperty __)
        {
            VisibilityChanged?.Invoke(this, new ModelInfoViewerVisibilityEventArgs(Visibility == Visibility.Visible));
        }

        private async void OpenInBrowserButton_Click(object _, RoutedEventArgs __)
        {
            await Launcher.LaunchUriAsync(WebView.Source);
            Hide();
        }

        private async void WebView_NavigationStarting(CoreWebView2 _, CoreWebView2NavigationStartingEventArgs args)
        {
            args.Cancel = true;
            await Launcher.LaunchUriAsync(new Uri(args.Uri));
        }
        private void WebView_PermissionRequested(CoreWebView2 sender, CoreWebView2PermissionRequestedEventArgs args)
        {
            args.State = CoreWebView2PermissionState.Deny;
            args.SavesInProfile = true;
            args.Handled = true;
        }
    }

    public record ModelInfoViewerVisibilityEventArgs(bool IsVisible);
}
