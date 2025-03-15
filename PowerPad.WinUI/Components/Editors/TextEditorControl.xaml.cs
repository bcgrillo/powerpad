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
using PowerPad.Core.Models;
using WinUIEditor;
using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.WinUI.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using CommunityToolkit.WinUI;
using PowerPad.Core.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PowerPad.WinUI.Components.Editors
{
    public sealed partial class TextEditorControl : EditorControl
    {
        private DocumentViewModel _document;

        public override bool IsDirty { get => _document.Status == DocumentStatus.Dirty; }

        public override DateTime LastSaveTime { get => _document.LastSaveTime; }

        public TextEditorControl(FolderEntryViewModel documentEntry)
        {
            this.InitializeComponent();

            TextEditor.Editor.WrapMode = Wrap.WhiteSpace;

            _document = documentEntry.ToDocumentViewModel(this);

            TextEditor.Editor.Modified += (s, e) => _document.Status = DocumentStatus.Dirty;
        }

        public override string GetContent()
        {
            return TextEditor.Editor.GetText(TextEditor.Editor.Length);
        }

        public override void SetContent(string content)
        {
            TextEditor.Editor.SetText(content);
        }

        private void EditableTextBlock_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            EditableTextBlock.Visibility = Visibility.Collapsed;
            EditableTextBox.Visibility = Visibility.Visible;
            EditableTextBox.Focus(FocusState.Programmatic);
        }

        private void EditableTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                FinalizeEditing();
            }
        }

        private void EditableTextBox_LostFocus(object sender, RoutedEventArgs e)
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
                InfoBar.Title = "Error";
                InfoBar.Message = "No ha sido posible cambiar el nombre del documento.";
                InfoBar.Visibility = Visibility.Visible;
                InfoBar.IsOpen = true;
            }
        }

        private void CopyBtn_Click(object sender, RoutedEventArgs e)
        {
            var textToCopy = TextEditor.Editor.GetText(TextEditor.Editor.Length);

            var dataPackage = new DataPackage();
            dataPackage.SetText(textToCopy);
            Clipboard.SetContent(dataPackage);
        }

        private void InfoBar_Closing(InfoBar sender, InfoBarClosingEventArgs args)
        {
            InfoBar.Visibility = Visibility.Collapsed;
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

        private async void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = await OllamaTestClass.GetResponseAsync(
                "llama3.2:latest",
                $"Eres un editor de textos, realizas la acción sin incluir mensajes adicionales como 'Aquí está lo que me has pedido' ni nada similar. Si se te pide una modificación debes devolver el contenido completo. Esta es tu orden actual: {InputBox.Text}",
                TextEditor.Editor.GetText(TextEditor.Editor.Length));

            TextEditor.Editor.SetText(result);
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
