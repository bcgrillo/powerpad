using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Models;
using PowerPad.WinUI.Components.Editors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PowerPad.WinUI.ViewModels
{
    public enum DocumentTypes
    {
        Text,
        Chat,
        Markdown,
        ToDo,
        Search
    }

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

        public DocumentTypes? DocumentType => _documentType;

        public IFolderEntry ModelEntry => _entry;

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
        }

        public FolderEntryViewModel(Document document)
        {
            _entry = document;

            Name = document.Name;
            Type = EntryType.Document;

            if (document.Path.EndsWith(".chat"))
            {
                _documentType = DocumentTypes.Chat;
                Glyph = "\U0000E15F"; //&#xE71C; //&#xE90A; //&#xE717;E90A
            }
            else
            {
                _documentType = DocumentTypes.Text;
                Glyph = "\U0000E70B";//"&#x;"; //&#xE8A6;
            }
        }

        public DocumentViewModel ToDocumentViewModel(IEditorContract editorControl)
        {
            return new DocumentViewModel((Document)_entry, editorControl);
        }
    }
}
