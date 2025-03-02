namespace PowerPad.Core.Models
{
    public class Folder : IFolderEntry
    {
        public string Name => System.IO.Path.GetFileName(Path)!;

        public EntryType Type => EntryType.Folder;

        public DocumentStatus Status => DocumentStatus.Saved;

        public string Path { get; set; }

        public IEnumerable<Folder> Folders { get; set; }

        public IEnumerable<Document> Documents { get; set; }

        public Folder(string path, IEnumerable<Folder> folders, IEnumerable<Document> documents)
        {
            Path = path;
            Folders = folders;
            Documents = documents;
        }
    }
}
