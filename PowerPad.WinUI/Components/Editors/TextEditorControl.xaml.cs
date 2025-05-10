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
    public partial class TextEditorControl : EditorControl
    {
        private DocumentViewModel _document;

        public override bool IsDirty { get => _document.Status == DocumentStatus.Dirty; }

        public override DateTime LastSaveTime { get => _document.LastSaveTime; }

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

        public override string GetContent(bool _ = false)
        {
            return TextEditor.Text;
        }

        public override void SetContent(string content)
        {
            TextEditor.Text = content;
            CopyBtn.IsEnabled = !string.IsNullOrEmpty(content);
            TextEditor_SizeChanged(null, null);
        }

        public override void SetFocus()
        {
            TextEditor.Focus(FocusState.Programmatic);
        }

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

        public override void AutoSave()
        {
            _document.AutosaveCommand.Execute(null);
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                _document = null!;
                TextEditor = null;
            }

            AgentControl.Dispose();
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

        public override int WordCount()
        {
            return TextEditor.Text.Split(' ').Length;
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
    }

    internal partial class ChatTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UserTemplate { get; set; } = null!;

        public DataTemplate AssistantTemplate { get; set; } = null!;

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
