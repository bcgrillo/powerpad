using PowerPad.Core.Contracts;
using System.Collections.ObjectModel;

namespace PowerPad.Core.Models.FileSystem
{
    /// <summary>
    /// Represents a folder in the file system, which can contain other folders and documents.
    /// </summary>
    public class Folder(string name) : IFolderEntry
    {
        private readonly Collection<Folder> _folders = [];
        private readonly Collection<Document> _documents = [];

        /// <inheritdoc />
        public string Name { get; set; } = name;

        /// <inheritdoc />
        public EntryType Type => EntryType.Folder;

        /// <inheritdoc />
        public DocumentStatus? Status => null;

        /// <inheritdoc />
        public Folder? Parent { get; internal set; }

        /// <inheritdoc />
        public virtual string Path => $"{Parent?.Path}\\{Name}";

        /// <summary>
        /// Gets the collection of subfolders within this folder.
        /// </summary>
        public ReadOnlyCollection<Folder>? Folders => _folders?.AsReadOnly();

        /// <summary>
        /// Gets the collection of documents within this folder.
        /// </summary>
        public ReadOnlyCollection<Document>? Documents => _documents?.AsReadOnly();

        /// <inheritdoc />
        public int? Position
        {
            get => Parent?.PositionOf(Name);
        }

        /// <summary>
        /// Gets or sets the order of entries within this folder.
        /// </summary>
        public List<string>? Order;

        /// <summary>
        /// Adds a subfolder to this folder.
        /// </summary>
        /// <param name="folder">The folder to add.</param>
        internal void AddFolder(Folder folder)
        {
            folder.Parent = this;
            _folders.Add(folder);
        }

        /// <summary>
        /// Adds a document to this folder.
        /// </summary>
        /// <param name="document">The document to add.</param>
        internal void AddDocument(Document document)
        {
            document.Parent = this;
            _documents.Add(document);
        }

        /// <summary>
        /// Removes a subfolder from this folder.
        /// </summary>
        /// <param name="folder">The folder to remove.</param>
        internal void RemoveFolder(Folder folder)
        {
            folder.Parent = null;
            _folders!.Remove(folder);
        }

        /// <summary>
        /// Removes a document from this folder.
        /// </summary>
        /// <param name="document">The document to remove.</param>
        internal void RemoveDocument(Document document)
        {
            document.Parent = null;
            _documents!.Remove(document);
        }

        /// <summary>
        /// Adds multiple subfolders to this folder.
        /// </summary>
        /// <param name="folders">The collection of folders to add.</param>
        internal void AddFolders(IEnumerable<Folder> folders)
        {
            foreach (var folder in folders)
            {
                folder.Parent = this;
                _folders.Add(folder);
            }
        }

        /// <summary>
        /// Adds multiple documents to this folder.
        /// </summary>
        /// <param name="documents">The collection of documents to add.</param>
        internal void AddDocuments(IEnumerable<Document> documents)
        {
            foreach (var document in documents)
            {
                document.Parent = this;
                _documents.Add(document);
            }
        }

        /// <summary>
        /// Gets the position of an entry by its name within this folder.
        /// </summary>
        /// <param name="entryName">The name of the entry.</param>
        /// <returns>The position of the entry, or null if not found.</returns>
        public int? PositionOf(string entryName)
        {
            var position = Order?.IndexOf(entryName);
            return position == -1 ? null : position;
        }
    }
}