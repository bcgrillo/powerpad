using System;
using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Models;
using CommunityToolkit.Mvvm.DependencyInjection;
using PowerPad.WinUI.ViewModels;

namespace PowerPad.WinUI.Components
{
    public sealed partial class WorkspaceControl : UserControl
    {
        public WorkspaceViewModel ViewModel => (WorkspaceViewModel)DataContext;

        public WorkspaceControl()
        {
            this.InitializeComponent();

            DataContext = Ioc.Default.GetRequiredService<WorkspaceViewModel>();
        }

        public event EventHandler<WorkspaceControlItemInvokedEventArgs>? ItemInvoked;

        private void TreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            var invokedFile = (FolderEntryViewModel)args.InvokedItem;

            if (invokedFile.Type == EntryType.Document)
            {
                ItemInvoked?.Invoke(this, new WorkspaceControlItemInvokedEventArgs(invokedFile));
            }
        }
    }

    public class WorkspaceControlItemInvokedEventArgs : EventArgs
    {
        public FolderEntryViewModel SelectedFile { get; }

        public WorkspaceControlItemInvokedEventArgs(FolderEntryViewModel selectedFile)
        {
            SelectedFile = selectedFile;
        }
    }
}
