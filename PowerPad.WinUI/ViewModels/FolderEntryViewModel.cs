using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using PowerPad.Core.Models;
using PowerPad.Core.Services;
using PowerPad.WinUI.Components.Editors;
using PowerPad.WinUI.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PowerPad.WinUI.ViewModels
{
    public partial class FolderEntryViewModel : ObservableObject
    {
        private readonly IFolderEntry _entry;
        private readonly DocumentTypes? _documentType;

        [ObservableProperty]
        public string _name;

        [ObservableProperty]
        public string? _glyph;

        [ObservableProperty]
        public EntryType _type;

        [ObservableProperty]
        public ObservableCollection<FolderEntryViewModel> _children;

        [ObservableProperty]
        public bool _renameStarted;

        [ObservableProperty]
        public bool _notRenameStarted = true;

        public DocumentTypes? DocumentType => _documentType;

        public IFolderEntry ModelEntry => _entry;

        public IRelayCommand StartRenameCommand { get; }

        public IRelayCommand DeleteCommand { get; }

        public FolderEntryViewModel(Folder folder)
        {
            _entry = folder;

            Name = folder.Name;
            Type = EntryType.Folder;

            Children = new ObservableCollection<FolderEntryViewModel>();

            if (folder.Folders != null)
            {
                foreach (var f in folder.Folders)
                {
                    Children.Add(new FolderEntryViewModel(f));
                }
            }

            if (folder.Documents != null)
            {
                foreach (var d in folder.Documents)
                {
                    Children.Add(new FolderEntryViewModel(d));
                }
            }

            StartRenameCommand = new RelayCommand(StartRename);
            DeleteCommand = new RelayCommand(Delete);
        }

        public FolderEntryViewModel(Document document)
        {
            _entry = document;

            Name = document.Name;
            Type = EntryType.Document;

            var documentType = DocumentTypeResolver.FromFilePath(document.Path);

            _documentType = documentType;
            Glyph = documentType.ToGlyph();

            StartRenameCommand = new RelayCommand(StartRename);
            DeleteCommand = new RelayCommand(Delete);
        }

        public DocumentViewModel ToDocumentViewModel(IEditorContract editorControl)
        {
            return new DocumentViewModel((Document)_entry, editorControl);
        }

        private void StartRename()
        {
            RenameStarted = true;
            NotRenameStarted = false;
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
        }
    }
}
