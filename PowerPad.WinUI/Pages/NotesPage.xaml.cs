using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Markup;
using System;
using PowerPad.Core.Services;
using PowerPad.Core.Models;
using System.IO;
using PowerPad.WinUI.Components;

namespace PowerPad.WinUI.Pages
{
    internal sealed partial class NotesPage : Page
    {
        public FileManager FileManager { get; set; } = FileManager.GetInstance();

        public NotesPage()
        {
            this.InitializeComponent();

            FileManager.SetWorkspace("D:\\OneDrive\\Escritorio\\Universidad\\PruebasTFG");
        }

        private void WorkspaceControl_ItemInvoked(object sender, TreeViewItemInvokedEventArgs e)
        {
            EditorManager.OpenFile(WorkspaceControl.SelectedFile!);
        }
    }
}