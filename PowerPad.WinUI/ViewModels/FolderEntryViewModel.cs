using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Models;
using PowerPad.WinUI.Components.Editors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.WinUI.ViewModels
{
    public partial class FolderEntryViewModel : ObservableObject
    {
        private readonly IFolderEntry _entry;

        [ObservableProperty]
        public string _name;

        [ObservableProperty]
        public string _glyph;

        [ObservableProperty]
        public EntryType _type;

        [ObservableProperty]
        public ObservableCollection<FolderEntryViewModel> _children;

        public FolderEntryViewModel(Folder folder)
        {
            _entry = folder;

            Name = folder.Name;
            Glyph = "&#xE8B7;";
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
            Glyph = "&#xE70B;";
            Type = EntryType.Document;
        }

        public DocumentViewModel ToDocumentViewModel(IEditorContract editorControl)
        {
            return new DocumentViewModel((Document)_entry, editorControl);
        }
    }
}
