using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using PowerPad.WinUI.Dialogs;
using PowerPad.WinUI.Helpers;
using PowerPad.WinUI.ViewModels.FileSystem;
using System;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace PowerPad.WinUI.Pages
{
    /// <summary>
    /// Represents a page for editing content in a popup editor.
    /// </summary>
    public sealed partial class PopupEditorPage : Page
    {
        private readonly WorkspaceViewModel _workspace;
        private readonly DraftDocumentViewModel _document;

        /// <summary>
        /// Gets the title bar of the popup editor.
        /// </summary>
        public Border TitleBar => BorderTitleBar;

        /// <summary>
        /// Event triggered when a request to close the popup editor is made.
        /// </summary>
        public event EventHandler? CloseRequested;

        /// <summary>
        /// Initializes a new instance of the <see cref="PopupEditorPage"/> class.
        /// </summary>
        public PopupEditorPage()
        {
            this.InitializeComponent();

            _workspace = App.Get<WorkspaceViewModel>();
            _document = new();
        }

        /// <summary>
        /// Sets the content of the document and clears undo/redo history.
        /// </summary>
        /// <param name="newContent">The new content to set.</param>
        public void SetContent(string newContent)
        {
            _document.PreviousContent = null;
            _document.NextContent = null;
            _document.Content = newContent;
        }

        /// <summary>
        /// Sets focus to the AgentControl.
        /// </summary>
        public void SetFocus() => AgentControl.SetFocus();

        /// <summary>
        /// Handles the size change event of the text editor to adjust padding based on scrollbar visibility.
        /// </summary>
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

        /// <summary>
        /// Finds a child element of a specified type within the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the element to find.</typeparam>
        /// <param name="element">The root element to search within.</param>
        /// <returns>The found element of type <typeparamref name="T"/>, or <c>null</c> if not found.</returns>
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

        /// <summary>
        /// Handles the click event of the Undo button to revert the document content to the previous state.
        /// </summary>
        private void UndoButton_Click(object _, RoutedEventArgs __)
        {
            _document.NextContent = _document.Content;
            _document.Content = _document.PreviousContent;
            _document.PreviousContent = null;
        }

        /// <summary>
        /// Handles the click event of the Redo button to restore the document content to the next state.
        /// </summary>
        private void RedoButton_Click(object _, RoutedEventArgs __)
        {
            _document.PreviousContent = _document.Content;
            _document.Content = _document.NextContent;
            _document.NextContent = null;
        }

        /// <summary>
        /// Handles the click event of the Copy button to copy the document content to the clipboard.
        /// </summary>
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="__">The event arguments (not used).</param>
        private async void CopyBtn_Click(object sender, RoutedEventArgs __)
        {
            var textToCopy = _document.Content;

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

        /// <summary>
        /// Handles the click event of the Apply button to copy the content and close the popup editor.
        /// </summary>
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void ApplyBtn_Click(object sender, RoutedEventArgs eventArgs)
        {
            CopyBtn_Click(sender, eventArgs);

            CloseRequested?.Invoke(this, EventArgs.Empty);

            HotKeyHelper.SimulateCtrlV();
        }

        /// <summary>
        /// Handles the Send button click event in the AgentControl to process and update the document content.
        /// </summary>
        private async void AgentControl_SendButtonClicked(object _, RoutedEventArgs __)
        {
            var originalText = _document.Content ?? string.Empty;
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

                _document.Content = TextEditor.Text;
            }
        }

        /// <summary>
        /// Handles the click event of the Save button to save the document as a new note.
        /// </summary>
        private void SaveBtn_Click(object _, RoutedEventArgs __)
        {
            App.MainWindow!.ShowNotes();

            _workspace.NewEntryCommand.Execute(NewEntryParameters.NewDocument(null, DocumentType.Note, null, _document.Content));

            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles the click event of the Hide button to close the popup editor.
        /// </summary>
        private void HideBtn_Click(object _, RoutedEventArgs __)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
