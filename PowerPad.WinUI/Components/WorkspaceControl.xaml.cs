using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using PowerPad.WinUI.Dialogs;
using System.Collections.Generic;
using Windows.Storage.Pickers;
using Windows.Storage;
using PowerPad.WinUI.Helpers;
using Windows.ApplicationModel.DataTransfer;
using PowerPad.WinUI.ViewModels.FileSystem;
using PowerPad.Core.Models.FileSystem;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Input;
using CommunityToolkit.Mvvm.Messaging;
using PowerPad.WinUI.Messages;
using System.Threading;

namespace PowerPad.WinUI.Components
{
    public partial class WorkspaceControl : UserControl, IRecipient<FolderEntryCreated>, IRecipient<FolderEntryChanged>
    {
        private static WorkspaceControl? _registredInstance = null;
        private readonly Lock _registredInstenceLock = new();

        private readonly WorkspaceViewModel _workspace;
        private readonly List<MenuFlyoutItem> _menuFlyoutItems;

        public event EventHandler<WorkspaceControlItemInvokedEventArgs>? ItemInvoked;
        public event EventHandler? VisibilityChanged;
        public WorkspaceControl()
        {
            this.InitializeComponent();

            _workspace = App.Get<WorkspaceViewModel>();
            _menuFlyoutItems = [];

            UpdateWorkspacesMenu();

            //TODO: Change for use dispose
            lock(_registredInstenceLock)
            {
                if (_registredInstance is not null)
                {
                    WeakReferenceMessenger.Default.Unregister<FolderEntryCreated>(_registredInstance);
                    WeakReferenceMessenger.Default.Unregister<FolderEntryChanged>(_registredInstance);
                }

                WeakReferenceMessenger.Default.Register<FolderEntryCreated>(this);
                WeakReferenceMessenger.Default.Register<FolderEntryChanged>(this);
                _registredInstance = this;
            }
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

                menuItem.Click += OpenRecentlyFlyoutItem_Click;
                WorkspaceMenu.Items.Add(menuItem);
                _menuFlyoutItems.Add(menuItem);
            }
        }

        private void TreeView_ItemInvoked(TreeView _, TreeViewItemInvokedEventArgs args)
        {
            var invokedEntry = (FolderEntryViewModel)args.InvokedItem;

            if (invokedEntry.Type == EntryType.Document)
            {
                ItemInvoked?.Invoke(this, new(invokedEntry));
                _workspace.CurrentDocumentPath = invokedEntry.ModelEntry.Path;
            }
            else
            {
                invokedEntry.IsExpanded = !invokedEntry.IsExpanded;
            }
        }

        private void TreeView_DragItemsCompleted(TreeView _, TreeViewDragItemsCompletedEventArgs args)
        {
            if (args.DropResult == DataPackageOperation.Move && args.Items.Count == 1 && args.Items[0] is FolderEntryViewModel entry)
            {
                var parentFolder = args.NewParentItem as FolderEntryViewModel;

                _workspace.MoveEntryCommand.Execute(new MoveEntryParameters(entry, parentFolder));
            }
        }

        private void HideMenuBtn_Click(object _, RoutedEventArgs __)
        {
            this.Visibility = Visibility.Collapsed;

            VisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

        private void NewChatButton_Click(object _, RoutedEventArgs __)
        {
            var parent = GetParentForNewElement();

            _workspace.NewEntryCommand.Execute(NewEntryParameters.NewDocument(parent, DocumentType.Chat));
        }

        private void NewNoteButton_Click(object _, RoutedEventArgs __)
        {
            var parent = GetParentForNewElement();

            _workspace.NewEntryCommand.Execute(NewEntryParameters.NewDocument(parent, DocumentType.Text));
        }

        private void NewFolderButton_Click(object _, RoutedEventArgs __)
        {
            var parent = GetParentForNewElement();

            _workspace.NewEntryCommand.Execute(NewEntryParameters.NewFolder(parent, "Nueva carpeta"));
        }

        private FolderEntryViewModel? GetParentForNewElement()
        {
            var parent = TreeView.SelectedItem as FolderEntryViewModel;
            if (parent is not null && parent.Type != EntryType.Folder) parent = (FolderEntryViewModel)TreeView.SelectedNode.Parent.Content;
            if (parent is not null) parent.IsExpanded = true;
            return parent;
        }

        private async void RenameFlyoutItem_Click(object sender, RoutedEventArgs __)
        {
            var entry = (FolderEntryViewModel)((MenuFlyoutItem)sender).DataContext;

            var result = await DialogHelper.Imput(this.XamlRoot, "Renombrar", "Nuevo nombre:", entry.Name);

            if (result is not null)
            {
                var newName = result;

                try
                {
                    entry.RenameCommand.Execute(newName);
                }
                catch
                {
                    await DialogHelper.Alert
                    (
                        this.XamlRoot,
                        "Error",
                        "No ha sido posible cambiar el nombre del documento."
                    );
                }
            }
        }

        private async void DeleteFlyoutItem_Click(object sender, RoutedEventArgs __)
        {
            var entry = (FolderEntryViewModel)((MenuFlyoutItem)sender).DataContext;

            var result = await DialogHelper.Confirm(this.XamlRoot, "Eliminar", "¿Estás seguro?");

            if (result)
            {
                try
                {
                    entry.DeleteCommand.Execute(null);
                }
                catch
                {
                    await DialogHelper.Alert
                    (
                        this.XamlRoot,
                        "Error",
                        "No ha sido posible eliminar el documento."
                    );
                }
            }
        }

        private async void OpenFolderFlyoutItem_Click(object _, RoutedEventArgs __)
        {
            var openPicker = new FolderPicker();

            var window = WindowHelper.GetWindowForElement(this);
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await openPicker.PickSingleFolderAsync();

            if (folder is not null)
            {
                _workspace.OpenWorkspaceCommand.Execute(folder.Path);
                ItemInvoked?.Invoke(this, new(null));

                UpdateWorkspacesMenu();
            }
        }

        private void OpenRecentlyFlyoutItem_Click(object sender, RoutedEventArgs __)
        {
            _workspace.OpenWorkspaceCommand.Execute(((MenuFlyoutItem)sender).Tag);
            ItemInvoked?.Invoke(this, new(null));

            UpdateWorkspacesMenu();
        }

        private static FolderEntryViewModel? FindFolderEntryByPathRecursive(IEnumerable<FolderEntryViewModel> entries, string path)
        {
            foreach (var entry in entries)
            {
                if (entry.ModelEntry.Path == path)
                {
                    return entry;
                }

                if (entry.IsFolder)
                {
                    var result = FindFolderEntryByPathRecursive(entry.Children, path);

                    if (result is not null)
                    {
                        entry.IsExpanded = true;
                        return result;
                    }
                }
            }

            return null;
        }

        private void TreeView_Loaded(object _, RoutedEventArgs __)
        {
            if (_workspace.CurrentDocumentPath is not null)
            {
                var entry = FindFolderEntryByPathRecursive(_workspace.Root.Children, _workspace.CurrentDocumentPath);

                if (entry is null) _workspace.CurrentDocumentPath = null;
                else
                {
                    DispatcherQueue.TryEnqueue(async () =>
                    {
                        //TODO: Check it, sometimes the entry is not selected because treeview is not loaded yet
                        for (int i = 0; i < 5; i++)
                        {
                            await Task.Delay(100);
                            entry.IsSelected = true;
                        }
                    });

                    ItemInvoked?.Invoke(this, new(entry));
                }
            }
        }

        private void TreeView_PointerPressed(object _, PointerRoutedEventArgs e)
        {
            var pointerPoint = e.GetCurrentPoint(TreeView);
            if (pointerPoint.Properties.PointerUpdateKind != PointerUpdateKind.RightButtonPressed) ClearSelection();
        }

        private void TreeView_KeyDown(object _, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape) ClearSelection();
        }

        public void Receive(FolderEntryCreated message)
        {
            ClearSelection();

            DispatcherQueue.TryEnqueue(async () =>
            {
                await Task.Delay(100);
                message.Value.IsSelected = true;
            });

            if (!message.Value.IsFolder)
            {
                ItemInvoked?.Invoke(this, new(message.Value));
                _workspace.CurrentDocumentPath = message.Value.ModelEntry.Path;
            }
        }

        public void Receive(FolderEntryChanged message)
        {
            var selectedItem = TreeView.SelectedItem as FolderEntryViewModel;

            if (message.NameChanged && selectedItem?.ModelEntry == message.Value)
            {
                _workspace.CurrentDocumentPath = message.Value.Path;
            }
        }

        public void ClearSelection() => ClearSelection(_workspace.Root.Children);

        private static void ClearSelection(IEnumerable<FolderEntryViewModel> entries)
        {
            foreach (var entry in entries)
            {
                entry.IsSelected = false;

                if (entry.IsFolder) ClearSelection(entry.Children);
            }
        }

        private void TreeView_SelectionChanged(TreeView _, TreeViewSelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is FolderEntryViewModel selectedEntry)
            {
                var container = (TreeViewItem)TreeView.ContainerFromItem(selectedEntry);
                container?.StartBringIntoView(new BringIntoViewOptions
                    {
                        AnimationDesired = true
                    });
            }
        }
    }

    public class WorkspaceControlItemInvokedEventArgs(FolderEntryViewModel? selectedFile) : EventArgs
    {
        public FolderEntryViewModel? SelectedFile { get; } = selectedFile;
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
