using System;
using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Models;
using CommunityToolkit.Mvvm.DependencyInjection;
using PowerPad.WinUI.ViewModels;
using Microsoft.UI.Xaml;
using PowerPad.WinUI.Dialogs;
using PowerPad.Core.Services;
using PowerPad.Core.Configuration;
using System.Collections.Generic;
using CommunityToolkit.WinUI;
using static System.Net.Mime.MediaTypeNames;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage;
using PowerPad.WinUI.Helpers;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Windows.Web.AtomPub;

namespace PowerPad.WinUI.Components
{
    public sealed partial class WorkspaceControl : UserControl
    {
        private readonly WorkspaceViewModel _workspace;
        private readonly List<MenuFlyoutItem> _menuFlyoutItems;

        public WorkspaceControl()
        {
            this.InitializeComponent();

            _workspace = new WorkspaceViewModel();
            _menuFlyoutItems = [];

            UpdateWorkspacesMenu();
        }

        private void UpdateWorkspacesMenu()
        {
            foreach (var item in _menuFlyoutItems) WorkspaceMenu.Items.Remove(item);
            _menuFlyoutItems.Clear();

            foreach (var item in _workspace.RecentlyWorkspaces)
            {
                var menuItem = new MenuFlyoutItem
                {
                    Text = item!.Split('\\')[^1],
                    Tag = item
                };

                ToolTipService.SetToolTip(menuItem, item);

                menuItem.Click += AbrirRecienteFlyoutItem_Click;
                WorkspaceMenu.Items.Add(menuItem);
                _menuFlyoutItems.Add(menuItem);
            }
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

        private async void AbrirCarpetaFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var openPicker = new FolderPicker();

            var window = WindowHelper.GetWindowForElement(this);
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await openPicker.PickSingleFolderAsync();

            if (folder != null)
            {
                _workspace.OpenWorkspaceCommand.Execute(folder.Path);

                UpdateWorkspacesMenu();
            }
        }

        private void AbrirRecienteFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            _workspace.OpenWorkspaceCommand.Execute(((MenuFlyoutItem)sender).Tag);

            UpdateWorkspacesMenu();
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
