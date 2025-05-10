using Microsoft.Extensions.AI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using PowerPad.Core.Models.FileSystem;
using PowerPad.WinUI.Dialogs;
using PowerPad.WinUI.ViewModels.Chat;
using PowerPad.WinUI.ViewModels.FileSystem;
using System;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace PowerPad.WinUI.Components.Editors
{
    /// <summary>
    /// Represents a text editor control that provides functionalities for editing, copying, and managing text content.
    /// </summary>
    public partial class TextEditorControl : EditorControl
    {
        private DocumentViewModel _document;

        /// <inheritdoc />
        public override bool IsDirty { get => _document.Status == DocumentStatus.Dirty; }

        /// <inheritdoc />
        public override DateTime LastSaveTime { get => _document.LastSaveTime; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextEditorControl"/> class.
        /// </summary>
        /// <param name="document">The document to be managed by the editor.</param>
        public TextEditorControl(Document document)
        {
            this.InitializeComponent();

            _document = new(document, this);

            TextEditor.TextChanged += (s, e) =>
            {
                _document.Status = DocumentStatus.Dirty;
                CopyBtn.IsEnabled = !string.IsNullOrEmpty(TextEditor.Text);
                TextEditor_SizeChanged(null, null);
            };
        }

        /// <inheritdoc />
        public override string GetContent(bool plainText = false)
        {
            return TextEditor.Text;
        }

        /// <inheritdoc />
        public override void SetContent(string content)
        {
            TextEditor.Text = content;
            CopyBtn.IsEnabled = !string.IsNullOrEmpty(content);
            TextEditor_SizeChanged(null, null);
        }

        /// <inheritdoc />
        public override void SetFocus()
        {
            TextEditor.Focus(FocusState.Programmatic);
        }

        /// <summary>
        /// Handles the event when the editable text block is edited.
        /// </summary>
        /// <param name="_">The sender of the event.</param>
        /// <param name="__">The event arguments.</param>
        private void EditableTextBlock_Edited(object _, EventArgs __)
        {
            try
            {
                _document.RenameCommand.Execute(EditableTextBlock.Value);
            }
            catch
            {
                DispatcherQueue.TryEnqueue(async () =>
                {
                    await DialogHelper.Alert
                    (
                        XamlRoot,
                        "Error",
                        "No ha sido posible cambiar el nombre del documento."
                    );
                });
            }
        }

        /// <summary>
        /// Handles the click event of the copy button.
        /// </summary>
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="__">The event arguments.</param>
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

        /// <inheritdoc />
        public override void AutoSave()
        {
            _document.AutosaveCommand.Execute(null);
        }

        /// <inheritdoc />
        public override int WordCount()
        {
            return TextEditor.Text.Split(' ').Length;
        }

        /// <summary>
        /// Handles the event when the send button in the agent control is clicked.
        /// </summary>
        /// <param name="_">The sender of the event.</param>
        /// <param name="__">The event arguments.</param>
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
                        XamlRoot,
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

        /// <summary>
        /// Handles the click event of the undo button.
        /// </summary>
        /// <param name="_">The sender of the event.</param>
        /// <param name="__">The event arguments.</param>
        private void UndoButton_Click(object _, RoutedEventArgs __)
        {
            _document.NextContent = TextEditor.Text;
            TextEditor.Text = _document.PreviousContent;
            _document.PreviousContent = null;
        }

        /// <summary>
        /// Handles the click event of the redo button.
        /// </summary>
        /// <param name="_">The sender of the event.</param>
        /// <param name="__">The event arguments.</param>
        private void RedoButton_Click(object _, RoutedEventArgs __)
        {
            _document.PreviousContent = TextEditor.Text;
            TextEditor.Text = _document.NextContent;
            _document.NextContent = null;
        }

        /// <summary>
        /// Handles the size changed event of the text editor.
        /// </summary>
        /// <param name="_">The sender of the event.</param>
        /// <param name="__">The event arguments.</param>
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
        /// Finds a child element of a specific type within the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the element to find.</typeparam>
        /// <param name="element">The parent element to search within.</param>
        /// <returns>The found element, or null if not found.</returns>
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

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _document = null!;
                TextEditor = null;
            }

            AgentControl.Dispose();
        }
    }

    /// <summary>
    /// Selects the appropriate data template for chat messages based on their role.
    /// </summary>
    internal partial class ChatTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Gets or sets the data template for user messages.
        /// </summary>
        public DataTemplate UserTemplate { get; set; } = null!;

        /// <summary>
        /// Gets or sets the data template for assistant messages.
        /// </summary>
        public DataTemplate AssistantTemplate { get; set; } = null!;

        /// <summary>
        /// Selects the appropriate data template based on the role of the message.
        /// </summary>
        /// <param name="item">The message to select a template for.</param>
        /// <param name="container">The container for the data template.</param>
        /// <returns>The selected data template.</returns>
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            MessageViewModel? selectedObject = item as MessageViewModel;

            if (selectedObject?.Role == ChatRole.User)
            {
                return UserTemplate;
            }
            else
            {
                return AssistantTemplate;
            }
        }
    }
}