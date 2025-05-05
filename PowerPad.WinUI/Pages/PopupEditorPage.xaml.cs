using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using PowerPad.Core.Contracts;
using PowerPad.WinUI.Helpers;
using PowerPad.WinUI.ViewModels.FileSystem;
using Microsoft.UI.Xaml.Media;
using PowerPad.WinUI.Dialogs;
using System.Text;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using System;

namespace PowerPad.WinUI.Pages
{
    public sealed partial class PopupEditorPage : Page
    {
        private readonly DraftDocumentViewModel _document;

        public PopupEditorPage()
        {
            this.InitializeComponent();

            _document = new();
        }

        private void TextEditor_SizeChanged(object? _, SizeChangedEventArgs? __)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                var scrollViewer = FindElement<ScrollViewer>(TextEditor);

                if (scrollViewer is not null)
                {
                    var isScrollbarVisible = scrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible;

                    if (isScrollbarVisible)
                    {
                        TextEditor.Padding = TextEditor.Padding with { Right = 16 };
                    }
                    else
                    {
                        TextEditor.Padding = TextEditor.Padding with { Right = 0 };
                    }
                }
            });
        }

        private T? FindElement<T>(DependencyObject element)
            where T : DependencyObject
        {
            if (element is T targetElement)
            {
                return targetElement;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                var result = FindElement<T>(child);
                if (result is not null)
                {
                    return result;
                }
            }

            return null;
        }

        private void UndoButton_Click(object _, RoutedEventArgs __)
        {
            _document.NextContent = TextEditor.Text;
            TextEditor.Text = _document.PreviousContent;
            _document.PreviousContent = null;
        }

        private void RedoButton_Click(object _, RoutedEventArgs __)
        {
            _document.PreviousContent = TextEditor.Text;
            TextEditor.Text = _document.NextContent;
            _document.NextContent = null;
        }

        private async void CopyBtn_Click(object sender, RoutedEventArgs __)
        {
            var textToCopy = TextEditor.Text;

            var dataPackage = new DataPackage();
            dataPackage.SetText(textToCopy);
            Clipboard.SetContent(dataPackage);

            var flyout = new Flyout
            {
                Content = new TextBlock
                {
                    Text = "¡Copiado!",
                    Padding = new Thickness(0),
                    Margin = new Thickness(-6),
                    TextAlignment = TextAlignment.Center
                },
                Placement = FlyoutPlacementMode.Top
            };

            flyout.ShowAt((Button)sender);
            await Task.Delay(1000);
            flyout.Hide();
        }

        private async void AgentControl_SendButtonClicked(object _, RoutedEventArgs __)
        {
            var originalText = TextEditor.Text;
            _document.PreviousContent = originalText;

            var hasSelection = TextEditor.SelectionLength > 0;
            var textToSend = hasSelection
                ? TextEditor.SelectedText
                : originalText;

            var resultBuilder = new StringBuilder();

            await AgentControl.StartAgentAction(textToSend, resultBuilder, (ex) =>
            {
                DispatcherQueue.TryEnqueue(async () =>
                {
                    await DialogHelper.Alert
                    (
                        Content.XamlRoot,
                        "Error",
                        ex.Message.Trim().ReplaceLineEndings(" ")
                    );
                });
            });

            var resultText = resultBuilder.ToString();

            if (!string.IsNullOrEmpty(resultText))
            {
                if (hasSelection) TextEditor.SelectedText = resultText;
                else TextEditor.Text = resultText;
            }
        }

        public void SetContent(string newContent) => TextEditor.Text = newContent;
    }
}
