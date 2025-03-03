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
    }
}
