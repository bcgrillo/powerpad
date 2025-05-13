using PowerPad.Core.Models.FileSystem;

namespace PowerPad.Core.Contracts
{
    /// <summary>
    /// Represents a generic entry in a folder structure, which can be either a folder or a document.
    /// </summary>
    public interface IFolderEntry
    {
        /// <summary>
        /// Gets the name of the folder or document.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the full path of the folder or document.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the type of the entry (e.g., Folder or Document).
        /// </summary>
        EntryType Type { get; }

        /// <summary>
        /// Gets the current status of the document, if applicable.
        /// </summary>
        DocumentStatus? Status { get; }

        /// <summary>
        /// Gets the parent folder of the entry, if it exists.
        /// </summary>
        Folder? Parent { get; }

        /// <summary>
        /// Gets the position of the entry within its parent folder, if applicable.
        /// </summary>
        int? Position { get; }
    }
}