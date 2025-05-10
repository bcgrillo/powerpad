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
    public partial class FolderEntryViewModel : ObservableObject, IRecipient<FolderEntryChanged>
    {
        private const string CLOSED_FOLDER_GLYPH = "\uE8B7";
        private const string OPEN_FOLDER_GLYPH = "\uE838";

        private readonly IFolderEntry _entry;
        private readonly DocumentType? _documentType;
        private readonly FolderEntryViewModel? _parent;

        public string Name { get => _entry.Name; }

        [ObservableProperty]
        public partial string? Glyph { get; set; }

        [ObservableProperty]
        public partial EntryType Type { get; set; }

        [ObservableProperty]
        public partial bool IsExpanded { get; set; }

        [ObservableProperty]
        public partial bool IsSelected { get; set; }

        public ObservableCollection<FolderEntryViewModel> Children { get; }

        public bool IsFolder => Type == EntryType.Folder;

        public DocumentType? DocumentType => _documentType;

        public IFolderEntry ModelEntry => _entry;

        public IRelayCommand DeleteCommand { get; }

        public IRelayCommand RenameCommand { get; }

        public int? Position { get => _entry.Position; }

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

        public void NameChanged()
        {
            WeakReferenceMessenger.Default.Send(new FolderEntryChanged(_entry) { NameChanged = true });

            OnPropertyChanged(nameof(Name));
        }

        public void Receive(FolderEntryChanged message)
        {
            if (message.Value == _entry && message.NameChanged)
            {
                OnPropertyChanged(nameof(Name));
            }
        }

        partial void OnIsExpandedChanged(bool value)
        {
            if (IsFolder) Glyph = value ? OPEN_FOLDER_GLYPH : CLOSED_FOLDER_GLYPH;
        }
    }
}
