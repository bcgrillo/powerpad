using PowerPad.Core.Contracts;
using PowerPad.Core.Services;

namespace PowerPad.Core.Models
{
    public class Document : IFolderEntry
    {
        public string Name { get; set; }

        public string Extension { get; set; }

        public EntryType Type => EntryType.Document;

        public DocumentStatus? Status { get; set; } = DocumentStatus.Unloaded;

        public Folder? Parent { get; internal set; }

        public string Path => $"{Parent?.Path}\\{Name}{Extension}";

        public string AutosavePath => AutosaveConventions.AutosavePath(Path);

        public Document(string name, string extension)
        {
            Name = name;
            Extension = extension;
        }
    }
}