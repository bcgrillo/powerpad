using System;
using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Models;
using CommunityToolkit.Mvvm.DependencyInjection;
using PowerPad.WinUI.ViewModels;
using Microsoft.UI.Xaml;
using PowerPad.WinUI.Dialogs;
using PowerPad.Core.Services;
using PowerPad.Core.Configuration;

namespace PowerPad.WinUI.Components
{
    public sealed partial class WorkspaceControl : UserControl
    {
        private readonly WorkspaceViewModel _workspace;

        public WorkspaceControl()
        {
            this.InitializeComponent();

            _workspace = new WorkspaceViewModel();
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

        private void HideMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void NewChatButton_Click(object sender, RoutedEventArgs e)
        {
            var parent = TreeView.SelectedItem as FolderEntryViewModel;

            if (parent != null && parent.Type != EntryType.Folder) parent = TreeView.SelectedNode.Parent.Content as FolderEntryViewModel;

            _workspace.NewEntryCommand.Execute(NewEntryParameters.NewDocument(parent, DocumentTypes.Chat));
        }

        private void NewSplitButton_Click(SplitButton sender, SplitButtonClickEventArgs args)
        {
            var parent = TreeView.SelectedItem as FolderEntryViewModel;

            if (parent != null && parent.Type != EntryType.Folder) parent = TreeView.SelectedNode.Parent.Content as FolderEntryViewModel;

            _workspace.NewEntryCommand.Execute(NewEntryParameters.NewDocument(parent, DocumentTypes.Text));
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var parent = TreeView.SelectedItem as FolderEntryViewModel;

            if (parent != null && parent.Type != EntryType.Folder) parent = TreeView.SelectedNode.Parent.Content as FolderEntryViewModel;

            var tag = ((MenuFlyoutItem)sender).Tag;

            if (tag == null)
            {
                _workspace.NewEntryCommand.Execute(NewEntryParameters.NewFolder(parent, "Nueva carpeta"));
            }   
            else
            {
                _workspace.NewEntryCommand.Execute(NewEntryParameters.NewDocument(parent, (DocumentTypes)tag));
            }
        }

        private async void RenameFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var entry = (FolderEntryViewModel)((MenuFlyoutItem)sender).DataContext;

            var result = await InputDialog.ShowAsync(this.XamlRoot, "Renombrar", entry.Name);

            if (result != null)
            {
                var newName = result;

                try
                {
                    entry.RenameCommand.Execute(newName);
                }
                catch (Exception)
                {
                }
            }
        }
    }

    public class WorkspaceControlItemInvokedEventArgs(FolderEntryViewModel selectedFile) : EventArgs
    {
        public FolderEntryViewModel SelectedFile { get; } = selectedFile;
    }

    public class EntryTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? FolderTemplate { get; set; }
        public DataTemplate? FileTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
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
