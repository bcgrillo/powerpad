using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using PowerPad.Core.Models.FileSystem;
using PowerPad.WinUI.Dialogs;
using PowerPad.WinUI.Messages;
using PowerPad.WinUI.ViewModels.FileSystem;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Pickers;

namespace PowerPad.WinUI.Components
{
    /// <summary>
    /// Represents a control for managing and interacting with the workspace, including folders and documents.
    /// </summary>
    public partial class WorkspaceControl : UserControl, IRecipient<FolderEntryCreated>, IRecipient<FolderEntryChanged>
    {
        private static WorkspaceControl? _activeInstance = null;
        private static readonly object _lock = new();

        private readonly WorkspaceViewModel _workspace;
        private readonly List<MenuFlyoutItem> _menuFlyoutItems;

        /// <summary>
        /// Occurs when an item in the workspace is invoked.
        /// </summary>
        public event EventHandler<WorkspaceControlItemInvokedEventArgs>? ItemInvoked;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkspaceControl"/> class.
        /// </summary>
        public WorkspaceControl()
        {
            this.InitializeComponent();

            _workspace = App.Get<WorkspaceViewModel>();
            _menuFlyoutItems = [];

            UpdateWorkspacesMenu();

            SetActiveInstance(this);
        }

        /// <summary>
        /// Sets the active instance of the <see cref="WorkspaceControl"/> and registers it for receiving messages.
        /// </summary>
        /// <param name="instance">The instance to set as active.</param>
        public static void SetActiveInstance(WorkspaceControl instance)
        {
            lock (_lock)
            {
                if (_activeInstance is not null)
                {
                    WeakReferenceMessenger.Default.Unregister<FolderEntryCreated>(_activeInstance);
                    WeakReferenceMessenger.Default.Unregister<FolderEntryChanged>(_activeInstance);
                }

                WeakReferenceMessenger.Default.Register<FolderEntryCreated>(instance);
                WeakReferenceMessenger.Default.Register<FolderEntryChanged>(instance);
                _activeInstance = instance;
            }
        }

        /// <summary>
        /// Receives a message indicating that a folder entry has been created.
        /// </summary>
        /// <param name="message">The message containing the created folder entry.</param>
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

        /// <summary>
        /// Receives a message indicating that a folder entry has been changed.
        /// </summary>
        /// <param name="message">The message containing the changed folder entry.</param>
        public void Receive(FolderEntryChanged message)
        {
            var selectedItem = TreeView.SelectedItem as FolderEntryViewModel;

            if (message.NameChanged && selectedItem?.ModelEntry == message.Value && selectedItem?.IsFolder == false)
            {
                _workspace.CurrentDocumentPath = message.Value.Path;
            }
        }

        /// <summary>
        /// Updates the workspace menu with recently accessed workspaces.
        /// </summary>
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

        /// <summary>
        /// Handles the event when an item in the tree view is invoked.
        /// </summary>
        /// <param name="_">The tree view instance (not used).</param>
        /// <param name="eventArgs">The event arguments containing the invoked item.</param>
        private void TreeView_ItemInvoked(TreeView _, TreeViewItemInvokedEventArgs eventArgs)
        {
            var invokedEntry = (FolderEntryViewModel)eventArgs.InvokedItem;

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

        /// <summary>
        /// Handles the event when drag-and-drop operation is completed in the tree view.
        /// </summary>
        /// <param name="_">The tree view instance (not used).</param>
        /// <param name="eventArgs">The event arguments containing drag-and-drop details.</param>
        private void TreeView_DragItemsCompleted(TreeView _, TreeViewDragItemsCompletedEventArgs eventArgs)
        {
            if (eventArgs.DropResult == DataPackageOperation.Move && eventArgs.Items.Count == 1 && eventArgs.Items[0] is FolderEntryViewModel entry)
            {
                var parentFolder = eventArgs.NewParentItem as FolderEntryViewModel;

                _workspace.MoveEntryCommand.Execute(new MoveEntryParameters(entry, parentFolder));
            }
        }

        /// <summary>
        /// Handles the click event for creating a new chat document.
        /// </summary>
        private void NewChatButton_Click(object _, RoutedEventArgs __)
        {
            var parent = GetParentForNewElement();

            _workspace.NewEntryCommand.Execute(NewEntryParameters.NewDocument(parent, DocumentType.Chat));
        }

        /// <summary>
        /// Handles the click event for creating a new note document.
        /// </summary>
        private void NewNoteButton_Click(object _, RoutedEventArgs __)
        {
            var parent = GetParentForNewElement();

            _workspace.NewEntryCommand.Execute(NewEntryParameters.NewDocument(parent, DocumentType.Note));
        }

        /// <summary>
        /// Handles the click event for creating a new folder.
        /// </summary>
        private void NewFolderButton_Click(object _, RoutedEventArgs __)
        {
            var parent = GetParentForNewElement();

            _workspace.NewEntryCommand.Execute(NewEntryParameters.NewFolder(parent, "Nueva carpeta"));
        }

        /// <summary>
        /// Gets the parent folder for creating a new element.
        /// </summary>
        /// <returns>The parent folder entry view model.</returns>
        private FolderEntryViewModel? GetParentForNewElement()
        {
            var parent = TreeView.SelectedItem as FolderEntryViewModel;
            if (parent is not null && parent.Type != EntryType.Folder) parent = (FolderEntryViewModel)TreeView.SelectedNode.Parent.Content;
            parent?.IsExpanded = true;
            return parent;
        }

        /// <summary>
        /// Handles the click event for renaming a folder or document.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="__">The event arguments (not used).</param>
        private async void RenameFlyoutItem_Click(object sender, RoutedEventArgs __)
        {
            var entry = (FolderEntryViewModel)((MenuFlyoutItem)sender).DataContext;

            var newName = await DialogHelper.Input(XamlRoot, "Renombrar", "Nuevo nombre:", entry.Name);

            if (newName is not null)
            {
                try
                {
                    entry.RenameCommand.Execute(newName);
                }
                catch
                {
                    await DialogHelper.Alert
                    (
                        XamlRoot,
                        "Error",
                        "No ha sido posible cambiar el nombre del documento."
                    );
                }
            }
        }

        /// <summary>
        /// Handles the click event for deleting a folder or document.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="__">The event arguments (not used).</param>
        private async void DeleteFlyoutItem_Click(object sender, RoutedEventArgs __)
        {
            var entry = (FolderEntryViewModel)((MenuFlyoutItem)sender).DataContext;

            var result = await DialogHelper.Confirm(XamlRoot, "Eliminar", "¿Está seguro?");

            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    entry.DeleteCommand.Execute(null);
                }
                catch
                {
                    await DialogHelper.Alert
                    (
                        XamlRoot,
                        "Error",
                        "No ha sido posible eliminar el documento."
                    );
                }
            }
        }

        /// <summary>
        /// Handles the click event for opening a folder using a folder picker.
        /// </summary>
        private async void OpenFolderFlyoutItem_Click(object _, RoutedEventArgs __)
        {
            var openPicker = new FolderPicker();

            var window = App.MainWindow;
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add("*");

            var folder = await openPicker.PickSingleFolderAsync();

            if (folder is not null)
            {
                _workspace.OpenWorkspaceCommand.Execute(folder.Path);
                ItemInvoked?.Invoke(this, new(null));

                UpdateWorkspacesMenu();
            }
        }

        /// <summary>
        /// Handles the click event for opening a recently accessed workspace.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="__">The event arguments (not used).</param>
        private void OpenRecentlyFlyoutItem_Click(object sender, RoutedEventArgs __)
        {
            _workspace.OpenWorkspaceCommand.Execute(((MenuFlyoutItem)sender).Tag);
            ItemInvoked?.Invoke(this, new(null));

            UpdateWorkspacesMenu();
        }

        /// <summary>
        /// Recursively finds a folder entry by its path.
        /// </summary>
        /// <param name="entries">The collection of folder entries to search.</param>
        /// <param name="path">The path of the folder entry to find.</param>
        /// <returns>The folder entry view model if found; otherwise, null.</returns>
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

        /// <summary>
        /// Handles the loaded event of the tree view.
        /// </summary>
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
                        await Task.Delay(100);
                        entry.IsSelected = true;
                    });

                    ItemInvoked?.Invoke(this, new(entry));
                }
            }
        }

        /// <summary>
        /// Handles the pointer pressed event of the tree view.
        /// </summary>
        private void TreeView_PointerPressed(object _, PointerRoutedEventArgs eventArgs)
        {
            var pointerPoint = eventArgs.GetCurrentPoint(TreeView);
            if (pointerPoint.Properties.PointerUpdateKind != PointerUpdateKind.RightButtonPressed) ClearSelection();
        }

        /// <summary>
        /// Handles the key down event of the tree view.
        /// </summary>
        private void TreeView_KeyDown(object _, KeyRoutedEventArgs eventArgs)
        {
            if (eventArgs.Key == Windows.System.VirtualKey.Escape) ClearSelection();
        }

        /// <summary>
        /// Clears the selection of all folder entries in the workspace.
        /// </summary>
        private void ClearSelection() => ClearSelection(_workspace.Root.Children);

        /// <summary>
        /// Recursively clears the selection of folder entries.
        /// </summary>
        /// <param name="entries">The collection of folder entries to clear selection.</param>
        private static void ClearSelection(IEnumerable<FolderEntryViewModel> entries)
        {
            foreach (var entry in entries)
            {
                entry.IsSelected = false;

                if (entry.IsFolder) ClearSelection(entry.Children);
            }
        }

        /// <summary>
        /// Handles the selection changed event of the tree view.
        /// </summary>
        private void TreeView_SelectionChanged(TreeView _, TreeViewSelectionChangedEventArgs eventArgs)
        {
            if (eventArgs.AddedItems.Count > 0 && eventArgs.AddedItems[0] is FolderEntryViewModel selectedEntry)
            {
                var container = (TreeViewItem)TreeView.ContainerFromItem(selectedEntry);
                container?.StartBringIntoView(new BringIntoViewOptions
                {
                    AnimationDesired = true
                });
            }
        }

        /// <summary>
        /// Handles the click event for showing the context menu of a tree view item.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="__">The event arguments (not used).</param>
        private void MoreButton_Click(object sender, RoutedEventArgs __)
        {
            var button = sender as Button;

            var container = TreeView.ContainerFromItem(button?.DataContext) as TreeViewItem;

            if (container?.ContextFlyout is MenuFlyout flyout) flyout.ShowAt(button);
        }
    }

    /// <summary>
    /// Provides event arguments for the <see cref="WorkspaceControl.ItemInvoked"/> event.
    /// </summary>
    public class WorkspaceControlItemInvokedEventArgs(FolderEntryViewModel? selectedFile) : EventArgs
    {
        /// <summary>
        /// Gets the selected file or folder entry.
        /// </summary>
        public FolderEntryViewModel? SelectedFile { get; } = selectedFile;
    }

    /// <summary>
    /// Selects a data template based on the type of folder entry (folder or file).
    /// </summary>
    public class EntryTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Gets or sets the data template for folders.
        /// </summary>
        public DataTemplate? FolderTemplate { get; set; }

        /// <summary>
        /// Gets or sets the data template for files.
        /// </summary>
        public DataTemplate? FileTemplate { get; set; }

        /// <summary>
        /// Selects the appropriate data template based on the type of the item.
        /// </summary>
        /// <param name="item">The item to select a template for.</param>
        /// <param name="container">The container for the item.</param>
        /// <returns>The selected data template.</returns>
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

    /// <summary>
    /// Represents the options available in the context menu.
    /// </summary>
    public enum ContextMenuOptions
    {
        /// <summary>Option to rename an entry.</summary>
        Rename,
        /// <summary>Option to delete an entry.</summary>
        Delete
    }
}