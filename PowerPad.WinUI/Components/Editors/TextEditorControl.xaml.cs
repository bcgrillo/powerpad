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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PowerPad.WinUI.Components.Editors
{
    public sealed partial class TextEditorControl : EditorControl
    {
        public TextEditorControl(FolderEntryViewModel documentEntry)
        {
            this.InitializeComponent();

            this.DataContext = documentEntry.ToDocumentViewModel(this);
        }

        public override string GetContent()
        {
            return TextBox.Text;
        }

        public override void SetContent(string content)
        {
            TextBox.Text = content;
        }

        private void TextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if (DataContext != null)
                ((DocumentViewModel)DataContext).Status = DocumentStatus.Dirty;
        }
    }
}
