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
using System.Collections.ObjectModel;
using PowerPad.Core.Services;
using Microsoft.UI.Xaml.Shapes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PowerPad.WinUI.Components
{
    public sealed partial class WorkspaceControl : UserControl
    {
        public FileManager FileManager { get; set; } = FileManager.GetInstance();
        public FileItem? SelectedFile { get; set; }

        public WorkspaceControl()
        {
            this.InitializeComponent();
        }
        public event EventHandler<TreeViewItemInvokedEventArgs>? ItemInvoked;

        private void TreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            var invokedFile = (FileItem)args.InvokedItem;

            if (!invokedFile.IsFolder)
            {
                SelectedFile = invokedFile;

                ItemInvoked?.Invoke(this, args);
            }
        }
    }
}
