using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.Extensions.AI;
using PowerPad.WinUI.Dialogs;
using PowerPad.WinUI.ViewModels.FileSystem;
using PowerPad.WinUI.ViewModels.Chat;
using PowerPad.Core.Models.FileSystem;
using PowerPad.Core.Services.AI;
using System.Text;

namespace PowerPad.WinUI.Components.Editors
{
    public partial class TextEditorControl : EditorControl
    {
        private DocumentViewModel _document;

        private readonly IChatService _chatService;

        public override bool IsDirty { get => _document.Status == DocumentStatus.Dirty; }

        public override DateTime LastSaveTime { get => _document.LastSaveTime; }

        public TextEditorControl(Document document)
        {
            _chatService = App.Get<IChatService>();

            this.InitializeComponent();

            _document = new(document, this);

            TextEditor.TextChanged += (s, e) => _document.Status = DocumentStatus.Dirty;
        }

        public override string GetContent()
        {
            return TextEditor.Text;
        }

        public override void SetContent(string content)
        {
            TextEditor.Text = content;
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
                EditableTextBlock.Value = _document.Name;

                DispatcherQueue.TryEnqueue(async () =>
                {
                    await DialogHelper.Alert
                    (
                        this.XamlRoot,
                        "Error",
                        "No ha sido posible cambiar el nombre del documento."
                    );
                });
            }
        }

        private void CopyBtn_Click(object _, RoutedEventArgs __)
        {
            var textToCopy = TextEditor.Text;

            var dataPackage = new DataPackage();
            dataPackage.SetText(textToCopy);
            Clipboard.SetContent(dataPackage);
        }

        public override void AutoSave()
        {
            _document.AutosaveCommand.Execute(null);
        }

        public override void Dispose()
        {
            _document = null!;
            TextEditor = null;

            AgentControl.Dispose();

            GC.SuppressFinalize(this);
        }

        private async void AgentControl_SendButtonClicked(object _, RoutedEventArgs __)
        {
            var stringBuilder = new StringBuilder();

            await AgentControl.StartAgentAction(TextEditor.Text, stringBuilder);

            TextEditor.Text = stringBuilder.ToString();
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
