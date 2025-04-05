using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Dialogs
{
    public sealed partial class DialogHelper : ContentDialog
    {
        private const string LABEL_OK = "Aceptar";
        private const string LABEL_CANCEL = "Cancelar";
        private const string LABEL_YES = "Sí";
        private const string LABEL_NO = "No";

        public bool Aceppted { get; private set; }

        private DialogHelper(Action<DialogHelper> formatter)
        {
            InitializeComponent();

            formatter(this);
        }

        public static async Task<string?> Imput(XamlRoot xamlRoot, string title, string message, string? currentValue, string primaryButtonText = LABEL_OK, string secondaryButtonText = LABEL_CANCEL)
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

            return inputDialog.Aceppted ? inputDialog.TextBox.Text : null;
        }

        public static async Task<bool> Confirm(XamlRoot xamlRoot, string title, string message, string primaryButtonText = LABEL_YES, string secondaryButtonText = LABEL_NO)
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

            await inputDialog.ShowAsync();

            return inputDialog.Aceppted;
        }

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
            });
            await inputDialog.ShowAsync();
        }

        private void TextBox_KeyDown(object _, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter) 
            {
                Aceppted = true;
                Hide();
            }
            else if(e.Key == Windows.System.VirtualKey.Escape)
            {
                Hide();
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog _, ContentDialogButtonClickEventArgs __)
        {
            Aceppted = true;
        }
    }
}
