using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using PowerPad.WinUI.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.Extensions.AI;
using Microsoft.UI.Input;
using PowerPad.WinUI.Dialogs;
using PowerPad.WinUI.ViewModels.FileSystem;
using PowerPad.WinUI.ViewModels.Chat;
using PowerPad.Core.Models.FileSystem;
using PowerPad.Core.Services.AI;
using PowerPad.Core.Models.AI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PowerPad.WinUI.Components.Editors
{
    public sealed partial class TextEditorControl : EditorControl
    {
        private DocumentViewModel _document;

        private readonly IChatService _chatService;

        public override bool IsDirty { get => _document.Status == DocumentStatus.Dirty; }

        public override DateTime LastSaveTime { get => _document.LastSaveTime; }

        public TextEditorControl(FolderEntryViewModel documentEntry, IChatService aiService)
        {
            this.InitializeComponent();

            _document = documentEntry.ToDocumentViewModel(this);

            TextEditor.TextChanged += (s, e) => _document.Status = DocumentStatus.Dirty;
            _chatService = aiService;
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

        private void EditableTextBlock_PointerPressed(object _, PointerRoutedEventArgs __)
        {
            EditableTextBlock.Visibility = Visibility.Collapsed;
            EditableTextBox.Visibility = Visibility.Visible;
            EditableTextBox.Focus(FocusState.Programmatic);
        }

        private void EditableTextBox_KeyDown(object _, KeyRoutedEventArgs args)
        {
            if (args.Key == Windows.System.VirtualKey.Enter)
            {
                FinalizeEditing();
            }
        }

        private void EditableTextBox_LostFocus(object _, RoutedEventArgs __)
        {
            FinalizeEditing();
        }

        private void FinalizeEditing()
        {
            EditableTextBlock.Visibility = Visibility.Visible;
            EditableTextBox.Visibility = Visibility.Collapsed;

            try
            {
                _document.RenameCommand.Execute(EditableTextBox.Text);
            }
            catch(Exception)
            {
                EditableTextBox.Text = _document.Name;

                DialogHelper.Alert
                (
                    this.XamlRoot,
                    "Error",
                    "No ha sido posible cambiar el nombre del documento."
                ).Wait();
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
        }

        private async void SendBtn_Click(object _, RoutedEventArgs __)
        {
            var result = await _chatService.GetResponse
            (
                message: TextEditor.Text == string.Empty ? " " : TextEditor.Text,
                config: new AIParameters
                {
                    SystemPrompt = $"Eres un editor de textos, realizas la acción sin incluir mensajes adicionales como 'Aquí está lo que me has pedido' ni nada similar. Si se te pide una modificación debes devolver el contenido completo. Esta es tu orden actual: {InputBox.Text}"
                }
            );

            TextEditor.Text = result.Text;
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
