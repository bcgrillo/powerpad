using PowerPad.Core.Contracts;
using PowerPad.Core.Services;

namespace PowerPad.Core.Models.FileSystem
{
    /// <summary>
    /// Represents a document within the file system. A document is a type of folder entry
    /// that has a name, extension, and can belong to a parent folder.
    /// </summary>
    public class Document(string name, string extension) : IFolderEntry
    {
        /// <inheritdoc />
        public string Name { get; set; } = name;

        /// <summary>
        /// Gets or sets the file extension of the document.
        /// </summary>
        public string Extension { get; set; } = extension;

        /// <inheritdoc />
        public EntryType Type => EntryType.Document;

        /// <inheritdoc />
        public DocumentStatus? Status { get; set; } = DocumentStatus.Unloaded;

        /// <inheritdoc />
        public Folder? Parent { get; internal set; }

        /// <inheritdoc />
        public string Path => $"{Parent?.Path}\\{Name}{Extension}";

        /// <summary>
        /// Gets the autosave path of the document, which is determined by the conventions service.
        /// </summary>
        public string AutosavePath => Conventions.AutosavePath(Path);

        /// <inheritdoc />
        public int? Position
        {
            get => Parent?.PositionOf($"{Name}{Extension}");
        }
    }
}