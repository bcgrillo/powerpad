using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;
using Windows.System;

namespace PowerPad.WinUI.Components.Controls
{
    /// <summary>
    /// Represents a control that displays model information in a WebView2 component.
    /// </summary>
    public partial class ModelInfoViewer : UserControl
    {
        /// <summary>
        /// Event triggered when the visibility of the control changes.
        /// </summary>
        public event EventHandler<ModelInfoViewerVisibilityEventArgs>? VisibilityChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelInfoViewer"/> class.
        /// </summary>
        public ModelInfoViewer()
        {
            this.InitializeComponent();

            RegisterPropertyChangedCallback(VisibilityProperty, OnVisibilityChanged);

            WebView.CoreWebView2Initialized += WebView_CoreWebView2Initialized;
        }

        /// <summary>
        /// Handles the initialization of the CoreWebView2 component.
        /// </summary>
        private void WebView_CoreWebView2Initialized(WebView2 _, CoreWebView2InitializedEventArgs __)
        {
            WebView.CoreWebView2.PermissionRequested += WebView_PermissionRequested;
        }

        /// <summary>
        /// Displays the control with the specified title and URL.
        /// </summary>
        /// <param name="title">The title to display.</param>
        /// <param name="url">The URL to load in the WebView2 component.</param>
        public void Show(string title, string url)
        {
            TitleTextBlock.Text = title;
            Visibility = Visibility.Visible;

            if (WebView.CoreWebView2 is not null) WebView.CoreWebView2.NavigationStarting -= WebView_NavigationStarting;

            WebView.Source = new Uri(url);
        }

        /// <summary>
        /// Hides the control and resets its state.
        /// </summary>
        public void Hide()
        {
            if (Visibility != Visibility.Collapsed)
            {
                Visibility = Visibility.Collapsed;
                LoadingSpinner.Visibility = Visibility.Visible;
                WebView.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Handles the click event of the close button to hide the control.
        /// </summary>
        private void CloseButton_Click(object _, RoutedEventArgs __) => Hide();

        /// <summary>
        /// Handles the completion of navigation in the WebView2 component.
        /// </summary>
        private void WebView_NavigationCompleted(WebView2 _, CoreWebView2NavigationCompletedEventArgs __)
        {
            WebView.Opacity = 0;
            LoadingSpinner.Visibility = Visibility.Collapsed;
            WebView.Visibility = Visibility.Visible;
            Task.Delay(100).Wait();
            WebView.Opacity = 1;

            WebView.CoreWebView2.NavigationStarting += WebView_NavigationStarting;
        }

        /// <summary>
        /// Handles changes to the visibility property of the control.
        /// </summary>
        private void OnVisibilityChanged(DependencyObject _, DependencyProperty __)
        {
            VisibilityChanged?.Invoke(this, new ModelInfoViewerVisibilityEventArgs(Visibility == Visibility.Visible));
        }

        /// <summary>
        /// Opens the current URL in the default browser and hides the control.
        /// </summary>
        private async void OpenInBrowserButton_Click(object _, RoutedEventArgs __)
        {
            await Launcher.LaunchUriAsync(WebView.Source);
            Hide();
        }

        /// <summary>
        /// Cancels navigation in the WebView2 component and opens the URL in the default browser.
        /// </summary>
        /// <param name="_"> The sender of the event (not used).</param>
        /// <param name="eventArgs">Event arguments for the navigation starting event.</param>
        private async void WebView_NavigationStarting(CoreWebView2 _, CoreWebView2NavigationStartingEventArgs eventArgs)
        {
            eventArgs.Cancel = true;
            await Launcher.LaunchUriAsync(new Uri(eventArgs.Uri));
        }

        /// <summary>
        /// Handles permission requests in the WebView2 component by denying them.
        /// </summary>
        /// <param name="_"> The sender of the event (not used).</param>
        /// <param name="eventArgs">Event arguments for the permission request.</param>
        private void WebView_PermissionRequested(CoreWebView2 _, CoreWebView2PermissionRequestedEventArgs eventArgs)
        {
            eventArgs.State = CoreWebView2PermissionState.Deny;
            eventArgs.SavesInProfile = true;
            eventArgs.Handled = true;
        }
    }

    /// <summary>
    /// Represents event arguments for the visibility change of the <see cref="ModelInfoViewer"/> control.
    /// </summary>
    /// <param name="IsVisible">Indicates whether the control is visible.</param>
    public record ModelInfoViewerVisibilityEventArgs(bool IsVisible);
}