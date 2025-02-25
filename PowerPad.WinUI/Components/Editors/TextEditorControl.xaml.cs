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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PowerPad.WinUI.Components.Editors
{
    public sealed partial class TextEditorControl : EditorControl
    {
        private FileStatus _fileStatus = FileStatus.Unloaded;
        private string _autoSavePath => FileItem.Path + ".autosave";


        public TextEditorControl(FileItem fileItem) : base(fileItem)
        {
            this.InitializeComponent();

            MyEditor.Editor.WrapMode = Wrap.WhiteSpace;

            Load();
        }

        public override void AutoSave()
        {
            File.WriteAllText(_autoSavePath, MyEditor.Editor.GetText(MyEditor.Editor.Length));
            _fileStatus = FileStatus.AutoSaved;
        }

        public override void Save()
        {
            File.WriteAllText(FileItem.Path, MyEditor.Editor.GetText(MyEditor.Editor.Length));
            if (File.Exists(_autoSavePath)) File.Delete(_autoSavePath);
            _fileStatus = FileStatus.Saved;
        }

        private void Load()
        {
            if (File.Exists(_autoSavePath))
            {
                MyEditor.Editor.SetText(File.ReadAllText(_autoSavePath));
                _fileStatus = FileStatus.AutoSaved;
            }
            else
            {
                MyEditor.Editor.SetText(File.ReadAllText(FileItem.Path));
                _fileStatus = FileStatus.Saved;
            }
        }
    }
}
