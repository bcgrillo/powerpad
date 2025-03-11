using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models;
using PowerPad.Core.Services;
using PowerPad.WinUI.Helpers;
using System;
using System.Collections.ObjectModel;

namespace PowerPad.WinUI.ViewModels
{
    public partial class FolderEntryViewModel : ObservableObject
    {
        private readonly IFolderEntry _entry;
        private readonly DocumentTypes? _documentType;

        private FolderEntryViewModel? _parent;

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

        public FolderEntryViewModel(Folder folder, FolderEntryViewModel? parent)
        {
            _entry = folder;
            _parent = parent;

            Type = EntryType.Folder;

            Children = new ObservableCollection<FolderEntryViewModel>();

            if (folder.Folders != null)
            {
                foreach (var f in folder.Folders)
                {
                    Children.Add(new FolderEntryViewModel(f, this));
                }
            }

            if (folder.Documents != null)
            {
                foreach (var d in folder.Documents)
                {
                    Children.Add(new FolderEntryViewModel(d, this));
                }
            }

            DeleteCommand = new RelayCommand(Delete);
            RenameCommand = new RelayCommand<string>(Rename);
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

            OnPropertyChanged(nameof(Name));
        }
    }
}
