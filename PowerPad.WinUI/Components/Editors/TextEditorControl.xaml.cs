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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PowerPad.WinUI.Components.Editors
{
    public sealed partial class TextEditorControl : EditorControl
    {
        public override bool IsDirty { get => ((DocumentViewModel)DataContext).Status == DocumentStatus.Dirty; }

        public override DateTime LastSaveTime { get => ((DocumentViewModel)DataContext).LastSaveTime; }

        public TextEditorControl(FolderEntryViewModel documentEntry)
        {
            this.InitializeComponent();

            TextEditor.Editor.WrapMode = Wrap.WhiteSpace;

            this.DataContext = documentEntry.ToDocumentViewModel(this);

            TextEditor.Editor.Modified += (s, e) => ((DocumentViewModel)DataContext).Status = DocumentStatus.Dirty;
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
                ((DocumentViewModel)DataContext).RenameCommand.Execute(EditableTextBox.Text);
            }
            catch(Exception)
            {
                EditableTextBox.Text = ((DocumentViewModel)DataContext).Name;
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

        private Microsoft.UI.Xaml.DependencyObject? FindChildElementByName(Microsoft.UI.Xaml.DependencyObject tree, string sName)
        {
            for (int i = 0; i < Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(tree); i++)
            {
                Microsoft.UI.Xaml.DependencyObject child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(tree, i);
                if (child != null && ((Microsoft.UI.Xaml.FrameworkElement)child).Name == sName)
                    return child;
                else
                {
                    Microsoft.UI.Xaml.DependencyObject? childInSubtree = FindChildElementByName(child, sName);
                    if (childInSubtree != null)
                        return childInSubtree;
                }
            }
            return null;
        }

        private void InfoBar_Closing(InfoBar sender, InfoBarClosingEventArgs args)
        {
            InfoBar.Visibility = Visibility.Collapsed;
        }

        public override void AutoSave()
        {
            ((DocumentViewModel)DataContext).AutosaveCommand.Execute(null);
        }

        public override void Dispose()
        {
            DataContext = null;
            TextEditor = null;
        }
    }
}
