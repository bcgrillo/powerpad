using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models;
using PowerPad.Core.Services;
using PowerPad.WinUI.Helpers;
using PowerPad.WinUI.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PowerPad.WinUI.ViewModels
{
    public partial class FolderEntryViewModel : ObservableObject, IRecipient<FolderEntryChanged>
    {
        private readonly IFolderEntry _entry;
        private readonly DocumentTypes? _documentType;

        private readonly FolderEntryViewModel? _parent;

        public string Name { get => _entry.Name; }

        [ObservableProperty]
        public string? _glyph;

        [ObservableProperty]
        public EntryType _type;

        [ObservableProperty]
        public ObservableCollection<FolderEntryViewModel>? _children;

        public bool IsFolder => Type == EntryType.Folder;

        public DocumentTypes? DocumentType => _documentType;

        public IFolderEntry ModelEntry => _entry;

        public IRelayCommand DeleteCommand { get; }

        public IRelayCommand RenameCommand { get; }

        public int? Position { get => _entry.Position; }

        public FolderEntryViewModel(Folder folder, FolderEntryViewModel? parent)
        {
            _entry = folder;
            _parent = parent;

            Type = EntryType.Folder;

            var children = new List<FolderEntryViewModel>();

            if (folder.Folders != null)
            {
                foreach (var f in folder.Folders)
                {
                    children.Add(new FolderEntryViewModel(f, this));
                }
            }

            if (folder.Documents != null)
            {
                foreach (var d in folder.Documents)
                {
                    children.Add(new FolderEntryViewModel(d, this));
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

            DeleteCommand = new RelayCommand(Delete);
            RenameCommand = new RelayCommand<string>(Rename);

            WeakReferenceMessenger.Default.Register(this);
        }

        public DocumentViewModel ToDocumentViewModel(IEditorContract editorControl)
        {
            return new DocumentViewModel((Document)_entry, editorControl);
        }

        private void Delete()
        {
            var workspaceService = Ioc.Default.GetRequiredService<IWorkspaceService>();

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
            ArgumentException.ThrowIfNullOrEmpty(newName, nameof(newName));

            var workspaceService = Ioc.Default.GetRequiredService<IWorkspaceService>();

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
            if (message.Value == _entry)
            {
                if (message.NameChanged) OnPropertyChanged(nameof(Name));
            }
        }
    }
}
