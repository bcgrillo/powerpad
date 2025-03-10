using System;
using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Models;
using CommunityToolkit.Mvvm.DependencyInjection;
using PowerPad.WinUI.ViewModels;
using Microsoft.UI.Xaml;

namespace PowerPad.WinUI.Components
{
    public sealed partial class WorkspaceControl : UserControl
    {
        public WorkspaceViewModel Workspace => (WorkspaceViewModel)DataContext;

        public WorkspaceControl()
        {
            this.InitializeComponent();

            DataContext = Ioc.Default.GetRequiredService<WorkspaceViewModel>();
        }

        public event EventHandler<WorkspaceControlItemInvokedEventArgs>? ItemInvoked;

        private void TreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            var invokedEntry = (FolderEntryViewModel)args.InvokedItem;

            if (invokedEntry.Type == EntryType.Document)
            {
                ItemInvoked?.Invoke(this, new WorkspaceControlItemInvokedEventArgs(invokedEntry));
            }
        }

        private void TreeViewItem_DropCompleted(Microsoft.UI.Xaml.UIElement sender, Microsoft.UI.Xaml.DropCompletedEventArgs args)
        {
            //var entry = args.OriginalSource as FolderEntryViewModel;
            //var newParent = (sender as TreeViewItem)?.DataContext as FolderEntryViewModel;

            //if (entry != null && newParent != null)
            //{
            //    ViewModel.MoveEntryCommand.Execute((entry, newParent));
            //}
        }

        private void HideMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void NewChatButton_Click(object sender, RoutedEventArgs e)
        {
            var parent = TreeView.SelectedItem as FolderEntryViewModel;

            if (parent != null && parent.Type != EntryType.Folder) parent = TreeView.SelectedNode.Parent.Content as FolderEntryViewModel;

            Workspace.NewEntryCommand.Execute(NewEntryParameters.NewDocument(parent, DocumentTypes.Chat, "Nuevo chat"));
        }

        private void NewSplitButton_Click(SplitButton sender, SplitButtonClickEventArgs args)
        {
            var parent = TreeView.SelectedItem as FolderEntryViewModel;

            if (parent != null && parent.Type != EntryType.Folder) parent = TreeView.SelectedNode.Parent.Content as FolderEntryViewModel;

            Workspace.NewEntryCommand.Execute(NewEntryParameters.NewDocument(parent, DocumentTypes.Text, "Nueva nota"));
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var parent = TreeView.SelectedItem as FolderEntryViewModel;

            if (parent != null && parent.Type != EntryType.Folder) parent = TreeView.SelectedNode.Parent.Content as FolderEntryViewModel;

            var tag = ((MenuFlyoutItem)sender).Tag;

            if (tag == null)
            {
                Workspace.NewEntryCommand.Execute(NewEntryParameters.NewFolder(parent, "Nueva carpeta"));
            }
            else
            {
                Workspace.NewEntryCommand.Execute(NewEntryParameters.NewDocument(parent, (DocumentTypes)tag, "Nuevo elemento"));
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

    public class EntryTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FolderTemplate { get; set; }
        public DataTemplate FileTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var entry = (FolderEntryViewModel)item;

            if (entry.Type == EntryType.Folder)
            {
                return FolderTemplate;
            }
            else
            {
                return FileTemplate;
            }
        }
    }

    public enum ContextMenuOptions
    {
        Rename,
        Delete
    }
}
