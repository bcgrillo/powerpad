using System.Collections.ObjectModel;

namespace PowerPad.Core.Models
{
    public class Folder : IFolderEntry
    {
        public string Name => System.IO.Path.GetFileName(Path)!;

        public EntryType Type => EntryType.Folder;

        public DocumentStatus Status => DocumentStatus.Saved;

        public IFolderEntry? Parent { get; set; }

        public string Path { get; set; }

        public Collection<Folder>? Folders { get; set; }

        public Collection<Document>? Documents { get; set; }

        public Folder(string path, IFolderEntry? parent)
        {
            Path = path;
            Parent = parent;
        }
    }
}
