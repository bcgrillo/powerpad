using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Dialogs
{
    /// <summary>
    /// Helper class for displaying various types of dialogs such as input, confirmation, and alerts.
    /// </summary>
    public partial class DialogHelper : ContentDialog
    {
        private const string LABEL_OK = "Aceptar";
        private const string LABEL_CANCEL = "Cancelar";
        private const string LABEL_YES = "Sí";
        private const string LABEL_NO = "No";

        private bool _accepted;

        /// <summary>
        /// Displays an input dialog to the user.
        /// </summary>
        /// <param name="xamlRoot">The XAML root for the dialog.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="message">The message to display in the dialog.</param>
        /// <param name="currentValue">The current value to prefill in the input box.</param>
        /// <param name="primaryButtonText">The text for the primary button. Defaults to "Aceptar".</param>
        /// <param name="secondaryButtonText">The text for the secondary button. Defaults to "Cancelar".</param>
        /// <returns>The input value if accepted, otherwise null.</returns>
        public static async Task<string?> Input(XamlRoot xamlRoot, string title, string message, string? currentValue, string primaryButtonText = LABEL_OK, string secondaryButtonText = LABEL_CANCEL)
        {
            var inputDialog = new DialogHelper(d =>
            {
                d.XamlRoot = xamlRoot;
                d.Title = title;
                d.PrimaryButtonText = primaryButtonText;
                d.SecondaryButtonText = secondaryButtonText;
                d.DefaultButton = ContentDialogButton.Primary;

                d.Message.Text = message;
                d.TextBox.Visibility = Visibility.Visible;
                d.TextBox.Text = currentValue ?? string.Empty;
                d.TextBox.SelectAll();
                d.TextBox.Focus(FocusState.Programmatic);
            });

            await inputDialog.ShowAsync();

            return inputDialog._accepted ? inputDialog.TextBox.Text : null;
        }

        /// <summary>
        /// Displays a confirmation dialog to the user.
        /// </summary>
        /// <param name="xamlRoot">The XAML root for the dialog.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="message">The message to display in the dialog.</param>
        /// <param name="primaryButtonText">The text for the primary button. Defaults to "Sí".</param>
        /// <param name="secondaryButtonText">The text for the secondary button. Defaults to "No".</param>
        /// <param name="showCancel">Indicates whether to show a cancel button. Defaults to false.</param>
        /// <returns>The result of the dialog interaction.</returns>
        public static async Task<ContentDialogResult> Confirm(XamlRoot xamlRoot, string title, string message, string primaryButtonText = LABEL_YES, string secondaryButtonText = LABEL_NO, bool showCancel = false)
        {
            var inputDialog = new DialogHelper(d =>
            {
                d.XamlRoot = xamlRoot;
                d.Title = title;
                d.PrimaryButtonText = primaryButtonText;
                d.SecondaryButtonText = secondaryButtonText;
                d.DefaultButton = ContentDialogButton.Secondary;

                d.Message.Text = message;
            });

            if (showCancel) inputDialog.CloseButtonText = "Cancelar";

            return await inputDialog.ShowAsync();
        }

        /// <summary>
        /// Displays an alert dialog to the user.
        /// </summary>
        /// <param name="xamlRoot">The XAML root for the dialog.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="message">The message to display in the dialog.</param>
        /// <param name="primaryButtonText">The text for the primary button. Defaults to "Aceptar".</param>
        public static async Task Alert(XamlRoot xamlRoot, string title, string message, string primaryButtonText = LABEL_OK)
        {
            var inputDialog = new DialogHelper(d =>
            {
                d.XamlRoot = xamlRoot;
                d.Title = title;
                d.PrimaryButtonText = primaryButtonText;
                d.DefaultButton = ContentDialogButton.Primary;
                d.Message.Text = message;

                d.Icon.Visibility = Visibility.Visible;
                d.Icon.Glyph = "\uE783";
                d.Icon.Foreground = (Brush)Application.Current.Resources["InfoBarErrorSeverityIconBackground"];
            });
            await inputDialog.ShowAsync();
        }

        /// <summary>
        /// Private constructor to initialize the dialog with a formatter.
        /// </summary>
        /// <param name="formatter">Action to configure the dialog instance.</param>
        private DialogHelper(Action<DialogHelper> formatter)
        {
            InitializeComponent();
            formatter(this);
        }

        /// <summary>
        /// Handles the KeyDown event for the TextBox to process Enter and Escape keys.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="eventArgs">The event arguments containing key information.</param>
        private void TextBox_KeyDown(object _, KeyRoutedEventArgs eventArgs)
        {
            if (eventArgs.Key == Windows.System.VirtualKey.Enter)
            {
                _accepted = true;
                Hide();
            }
            else if (eventArgs.Key == Windows.System.VirtualKey.Escape)
            {
                Hide();
            }
        }

        /// <summary>
        /// Handles the PrimaryButtonClick event to mark the dialog as accepted.
        /// </summary>
        private void ContentDialog_PrimaryButtonClick(ContentDialog _, ContentDialogButtonClickEventArgs __)
        {
            _accepted = true;
        }
    }
}