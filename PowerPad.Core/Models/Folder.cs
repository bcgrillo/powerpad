using System.Collections.ObjectModel;

namespace PowerPad.Core.Models
{
    public class Folder : IFolderEntry
    {
        private Collection<Folder>? _folders;
        private Collection<Document>? _documents;

        public string Name => System.IO.Path.GetFileName(Path)!;

        public EntryType Type => EntryType.Folder;

        public DocumentStatus Status => DocumentStatus.Saved;

        public Folder? Parent { get; internal set; }

        public string Path { get; set; }

        public ReadOnlyCollection<Folder>? Folders => _folders?.AsReadOnly();

        public ReadOnlyCollection<Document>? Documents => _documents?.AsReadOnly();

        public Folder(string path)
        {
            Path = path;
        }

        public void AddFolder(Folder folder)
        {
            if (_folders == null) _folders = new Collection<Folder>();

            folder.Parent = this;

            _folders.Add(folder);
        }

        public void AddDocument(Document document)
        {
            if (_documents == null) _documents = new Collection<Document>();

            document.Parent = this;

            _documents.Add(document);
        }

        public void RemoveFolder(Folder folder)
        {
            folder.Parent = null;
            _folders!.Remove(folder);
        }

        public void RemoveDocument(Document document)
        {
            document.Parent = null;
            _documents!.Remove(document);
        }

        public void AddFolders(IEnumerable<Folder> folders)
        {
            if (_folders == null) _folders = new Collection<Folder>();
            foreach (var folder in folders)
            {
                folder.Parent = this;
                _folders.Add(folder);
            }
        }

        public void AddDocuments(IEnumerable<Document> documents)
        {
            if (_documents == null) _documents = new Collection<Document>();
            foreach (var document in documents)
            {
                document.Parent = this;
                _documents.Add(document);
            }
        }
    }
}
