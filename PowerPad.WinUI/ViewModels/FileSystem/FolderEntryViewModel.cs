using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.FileSystem;
using PowerPad.Core.Services.FileSystem;
using PowerPad.WinUI.Helpers;
using PowerPad.WinUI.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PowerPad.WinUI.ViewModels.FileSystem
{
    /// <summary>
    /// ViewModel representing a folder or document entry in the file system.
    /// </summary>
    public partial class FolderEntryViewModel : ObservableObject, IRecipient<FolderEntryChanged>
    {
        private const string CLOSED_FOLDER_GLYPH = "\uE8B7";
        private const string OPEN_FOLDER_GLYPH = "\uE838";

        private readonly IFolderEntry _entry;
        private readonly DocumentType? _documentType;
        private readonly FolderEntryViewModel? _parent;

        /// <summary>
        /// Gets the name of the folder or document.
        /// </summary>
        public string Name { get => _entry.Name; }

        /// <summary>
        /// Gets or sets the glyph representing the folder or document.
        /// </summary>
        [ObservableProperty]
        public partial string? Glyph { get; set; }

        /// <summary>
        /// Gets or sets the type of the entry (Folder or Document).
        /// </summary>
        [ObservableProperty]
        public partial EntryType Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the folder is expanded.
        /// </summary>
        [ObservableProperty]
        public partial bool IsExpanded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entry is selected.
        /// </summary>
        [ObservableProperty]
        public partial bool IsSelected { get; set; }

        /// <summary>
        /// Gets the collection of child entries (folders or documents).
        /// </summary>
        public ObservableCollection<FolderEntryViewModel> Children { get; }

        /// <summary>
        /// Gets a value indicating whether the entry is a folder.
        /// </summary>
        public bool IsFolder => Type == EntryType.Folder;

        /// <summary>
        /// Gets the document type of the entry, if applicable.
        /// </summary>
        public DocumentType? DocumentType => _documentType;

        /// <summary>
        /// Gets the underlying model entry.
        /// </summary>
        public IFolderEntry ModelEntry => _entry;

        /// <summary>
        /// Gets the command to delete the entry.
        /// </summary>
        public IRelayCommand DeleteCommand { get; }

        /// <summary>
        /// Gets the command to rename the entry.
        /// </summary>
        public IRelayCommand RenameCommand { get; }

        /// <summary>
        /// Gets the position of the entry within its parent folder, if applicable.
        /// </summary>
        public int? Position { get => _entry.Position; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderEntryViewModel"/> class for a folder.
        /// </summary>
        /// <param name="folder">The folder model.</param>
        /// <param name="parent">The parent folder entry view model.</param>
        public FolderEntryViewModel(Folder folder, FolderEntryViewModel? parent)
        {
            _entry = folder;
            _parent = parent;

            Type = EntryType.Folder;

            Glyph = CLOSED_FOLDER_GLYPH;

            var children = new List<FolderEntryViewModel>();

            if (folder.Folders is not null)
            {
                foreach (var f in folder.Folders)
                {
                    children.Add(new(f, this));
                }
            }

            if (folder.Documents is not null)
            {
                foreach (var d in folder.Documents)
                {
                    children.Add(new(d, this));
                }
            }

            Children = [.. children.OrderBy(x => (x.Position.HasValue ? 0 : 1)).ThenBy(x => x.Position)];

            DeleteCommand = new RelayCommand(Delete);
            RenameCommand = new RelayCommand<string>(Rename);

            WeakReferenceMessenger.Default.Register(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderEntryViewModel"/> class for a document.
        /// </summary>
        /// <param name="document">The document model.</param>
        /// <param name="parent">The parent folder entry view model.</param>
        public FolderEntryViewModel(Document document, FolderEntryViewModel? parent)
        {
            _entry = document;
            _parent = parent;

            Type = EntryType.Document;

            var documentType = DocumentTypeResolver.FromFilePath(document.Path);

            _documentType = documentType;
            Glyph = documentType.ToGlyph();

            Children = [];
            Children.CollectionChanged += (s, e) => throw new InvalidOperationException("Documents cannot have child elements");

            DeleteCommand = new RelayCommand(Delete);
            RenameCommand = new RelayCommand<string>(Rename);

            WeakReferenceMessenger.Default.Register(this);
        }

        /// <summary>
        /// Notifies that the name of the entry has changed.
        /// </summary>
        public void NameChanged()
        {
            WeakReferenceMessenger.Default.Send(new FolderEntryChanged(_entry) { NameChanged = true });

            OnPropertyChanged(nameof(Name));
        }

        /// <summary>
        /// Handles the receipt of a <see cref="FolderEntryChanged"/> message.
        /// </summary>
        /// <param name="message">The message indicating the folder entry change.</param>
        public void Receive(FolderEntryChanged message)
        {
            if (message.Value == _entry && message.NameChanged)
            {
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// Updates the glyph when the expanded state of the folder changes.
        /// </summary>
        /// <param name="value">The new expanded state.</param>
        partial void OnIsExpandedChanged(bool value)
        {
            if (IsFolder) Glyph = value ? OPEN_FOLDER_GLYPH : CLOSED_FOLDER_GLYPH;
        }

        /// <summary>
        /// Deletes the folder or document entry.
        /// </summary>
        private void Delete()
        {
            var workspaceService = App.Get<IWorkspaceService>();

            if (Type == EntryType.Folder)
            {
                workspaceService.DeleteFolder((Folder)_entry);
            }
            else
            {
                workspaceService.DeleteDocument((Document)_entry);
            }

            _parent?.Children!.Remove(this);
            WeakReferenceMessenger.Default.Send(new FolderEntryDeleted(_entry));
        }

        /// <summary>
        /// Renames the folder or document entry.
        /// </summary>
        /// <param name="newName">The new name for the entry.</param>
        private void Rename(string? newName)
        {
            ArgumentException.ThrowIfNullOrEmpty(newName);

            var workspaceService = App.Get<IWorkspaceService>();

            if (Type == EntryType.Document)
            {
                workspaceService.RenameDocument((Document)_entry, newName);
            }
            else
            {
                workspaceService.RenameFolder((Folder)_entry, newName);
            }

            NameChanged();
        }
    }
}
