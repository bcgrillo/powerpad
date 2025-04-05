using PowerPad.Core.Contracts;
using PowerPad.Core.Services;

namespace PowerPad.Core.Models.FileSystem
{
    public class Document(string name, string extension) : IFolderEntry
    {
        public string Name { get; set; } = name;

        public string Extension { get; set; } = extension;

        public EntryType Type => EntryType.Document;

        public DocumentStatus? Status { get; set; } = DocumentStatus.Unloaded;

        public Folder? Parent { get; internal set; }

        public string Path => $"{Parent?.Path}\\{Name}{Extension}";

        public string AutosavePath => Conventions.AutosavePath(Path);

        public int? Position
        {
            get => Parent?.PositionOf($"{Name}{Extension}");
        }
    }
}