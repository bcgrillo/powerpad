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
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PowerPad.WinUI.Dialogs
{
    public sealed partial class InputDialog : ContentDialog
    {
        public bool Aceppted { get; private set; }

        private InputDialog(string? currentValue)
        {
            InitializeComponent();

            TextBox.Text = currentValue ?? string.Empty;
            TextBox.SelectAll();
            TextBox.Focus(FocusState.Programmatic);
        }

        public static async Task<string?> ShowAsync(XamlRoot xamlRoot, string title, string? currentValue, string primaryButtonText = "Aceptar", string secondaryButtonText = "Cancelar")
        {
            var inputDialog = new InputDialog(currentValue)
            {
                XamlRoot = xamlRoot,
                Title = title,
                PrimaryButtonText = primaryButtonText,
                SecondaryButtonText = secondaryButtonText,
                DefaultButton = ContentDialogButton.Primary
            };

            await inputDialog.ShowAsync();

            return inputDialog.Aceppted ? inputDialog.TextBox.Text : null;
        }

        private void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
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

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Aceppted = true;
        }
    }
}
