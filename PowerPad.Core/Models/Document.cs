namespace PowerPad.Core.Models
{
    public class Document : IFolderEntry
    {
        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path)!;

        public EntryType Type => EntryType.Document;

        public DocumentStatus Status { get; set; } = DocumentStatus.Unloaded;

        public Folder? Parent { get; internal set; }

        public string Path { get; set; }

        public Document(string path)
        {
            Path = path;
        }
    }
}