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
        public NotesPage()
        {
            this.InitializeComponent();
        }

        private void WorkspaceControl_ItemInvoked(object sender, WorkspaceControlItemInvokedEventArgs e)
        {
            EditorManager.OpenFile(e.SelectedFile);
        }
    }
}