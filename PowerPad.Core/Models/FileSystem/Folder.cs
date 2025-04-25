using PowerPad.Core.Contracts;
using System.Collections.ObjectModel;

namespace PowerPad.Core.Models.FileSystem
{
    public class Folder(string name) : IFolderEntry
    {
        private readonly Collection<Folder> _folders = [];
        private readonly Collection<Document> _documents = [];

        public string Name { get; set; } = name;

        public EntryType Type => EntryType.Folder;

        public DocumentStatus? Status => null;

        public Folder? Parent { get; internal set; }

        public virtual string Path => $"{Parent?.Path}\\{Name}";

        public ReadOnlyCollection<Folder>? Folders => _folders?.AsReadOnly();

        public ReadOnlyCollection<Document>? Documents => _documents?.AsReadOnly();

        public int? Position
        {
            get => Parent?.PositionOf(Name);
        }

        public List<string>? Order;

        internal void AddFolder(Folder folder)
        {
            folder.Parent = this;

            _folders.Add(folder);
        }

        internal void AddDocument(Document document)
        {
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
            foreach (var folder in folders)
            {
                folder.Parent = this;
                _folders.Add(folder);
            }
        }

        internal void AddDocuments(IEnumerable<Document> documents)
        {
            foreach (var document in documents)
            {
                document.Parent = this;
                _documents.Add(document);
            }
        }

        public int? PositionOf(string entryName)
        {
            var position = Order?.IndexOf(entryName);

            return position == -1 ? null : position;
        }
    }
}