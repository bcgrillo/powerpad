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
        public TextContent TextContent { get; set; }

        private string _autoSavePath => FileItem.Path + ".autosave";

        public TextEditorControl(FileItem fileItem) : base(fileItem)
        {
            this.InitializeComponent();

            TextContent = new TextContent();
            this.DataContext = TextContent;

            Load();
        }

        public override void AutoSave()
        {
            File.WriteAllText(_autoSavePath, TextContent.Content);
            TextContent.Status = FileStatus.AutoSaved;
        }

        public override void Save()
        {
            File.WriteAllText(FileItem.Path, TextContent.Content);
            if (File.Exists(_autoSavePath)) File.Delete(_autoSavePath);
            TextContent.Status = FileStatus.Saved;
        }

        private void Load()
        {
            if (File.Exists(_autoSavePath))
            {
                TextContent.Content = File.ReadAllText(_autoSavePath);
                TextContent.Status = FileStatus.AutoSaved;
            }
            else
            {
                TextContent.Content = File.ReadAllText(FileItem.Path);
                TextContent.Status = FileStatus.Saved;
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }
    }
}
