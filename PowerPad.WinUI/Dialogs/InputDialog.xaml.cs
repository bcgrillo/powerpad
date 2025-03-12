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
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(InputDialog), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty DialogTitleProperty = DependencyProperty.Register(
            "DialogTitle", typeof(string), typeof(InputDialog), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty PrimaryButtonTextProperty = DependencyProperty.Register(
            "PrimaryButtonText", typeof(string), typeof(InputDialog), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty SecondaryButtonTextProperty = DependencyProperty.Register(
            "SecondaryButtonText", typeof(string), typeof(InputDialog), new PropertyMetadata(default(string)));

        private InputDialog(XamlRoot xamlRoot)
        {
            InitializeComponent();
            this.XamlRoot = xamlRoot;
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string DialogTitle
        {
            get { return (string)GetValue(DialogTitleProperty); }
            set { SetValue(DialogTitleProperty, value); }
        }

        public string PrimaryButtonText
        {
            get { return (string)GetValue(PrimaryButtonTextProperty); }
            set { SetValue(PrimaryButtonTextProperty, value); }
        }

        public string SecondaryButtonText
        {
            get { return (string)GetValue(SecondaryButtonTextProperty); }
            set { SetValue(SecondaryButtonTextProperty, value); }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        public static async Task<string?> ShowAsync(XamlRoot xamlRoot, string title, string? currentValue, string primaryButtonText = "Aceptar", string secondaryButtonText = "Cancelar")
        {
            var inputDialog = new InputDialog(xamlRoot)
            {
                Title = title,
                Text = currentValue ?? string.Empty,
                PrimaryButtonText = primaryButtonText,
                SecondaryButtonText = secondaryButtonText
            };

            var dialogResult = await inputDialog.ShowAsync();

            return dialogResult == ContentDialogResult.Primary ? inputDialog.Text : null;
        }
    }
}
