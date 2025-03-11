using PowerPad.Core.Contracts;
using System.Collections.ObjectModel;

namespace PowerPad.Core.Models
{
    public class Folder : IFolderEntry
    {
        private string? _rootPath;

        private Collection<Folder>? _folders;
        private Collection<Document>? _documents;

        public string Name { get; set; }

        public EntryType Type => EntryType.Folder;

        public DocumentStatus? Status => null;

        public Folder? Parent { get; internal set; }

        public string Path => _rootPath ?? $"{Parent?.Path}\\{Name}";

        public ReadOnlyCollection<Folder>? Folders => _folders?.AsReadOnly();

        public ReadOnlyCollection<Document>? Documents => _documents?.AsReadOnly();

        private Folder()
        {
            Name = null!;
        }

        public Folder(string name)
        {
            Name = name;
        }

        internal void AddFolder(Folder folder)
        {
            if (_folders == null) _folders = new Collection<Folder>();

            folder.Parent = this;

            _folders.Add(folder);
        }

        internal void AddDocument(Document document)
        {
            if (_documents == null) _documents = new Collection<Document>();

            document.Parent = this;

            _documents.Add(document);
        }

        internal void RemoveFolder(Folder folder)
        {
            folder.Parent = null;
            _folders!.Remove(folder);
        }

        internal void RemoveDocument(Document document)
        {
            document.Parent = null;
            _documents!.Remove(document);
        }

        internal void AddFolders(IEnumerable<Folder> folders)
        {
            if (_folders == null) _folders = new Collection<Folder>();
            foreach (var folder in folders)
            {
                folder.Parent = this;
                _folders.Add(folder);
            }
        }

        internal void AddDocuments(IEnumerable<Document> documents)
        {
            if (_documents == null) _documents = new Collection<Document>();
            foreach (var document in documents)
            {
                document.Parent = this;
                _documents.Add(document);
            }
        }

        public static Folder CreateRoot(string rootPath)
        {
            var root = new Folder();
            root._rootPath = rootPath;
            return root;
        }
    }
}