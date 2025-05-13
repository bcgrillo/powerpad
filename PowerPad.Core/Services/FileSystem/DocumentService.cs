using PowerPad.Core.Contracts;
using PowerPad.Core.Models.FileSystem;

namespace PowerPad.Core.Services.FileSystem
{
    /// <summary>
    /// Provides methods to manage and manipulate documents in the file system.
    /// </summary>
    public interface IDocumentService
    {
        /// <summary>
        /// Loads the content of a document into the editor.
        /// </summary>
        /// <param name="document">The document to load.</param>
        /// <param name="editor">The editor where the document content will be loaded.</param>
        void LoadDocument(Document document, IEditorContract editor);

        /// <summary>
        /// Autosaves the content of a document to its autosave path.
        /// </summary>
        /// <param name="document">The document to autosave.</param>
        /// <param name="editor">The editor containing the document content.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AutosaveDocument(Document document, IEditorContract editor);

        /// <summary>
        /// Saves the content of a document to its primary path and removes any autosave file.
        /// </summary>
        /// <param name="document">The document to save.</param>
        /// <param name="editor">The editor containing the document content.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SaveDocument(Document document, IEditorContract editor);
    }

    /// <summary>
    /// Implementation of <see cref="IDocumentService"/> to manage document operations.
    /// </summary>
    public class DocumentService : IDocumentService
    {
        /// <inheritdoc />
        public void LoadDocument(Document document, IEditorContract editor)
        {
            var autosaveExists = File.Exists(document.AutosavePath);

            if (autosaveExists)
            {
                editor.SetContent(File.ReadAllText(document.AutosavePath));
                document.Status = DocumentStatus.AutoSaved;
            }
            else
            {
                editor.SetContent(File.ReadAllText(document.Path));
                document.Status = DocumentStatus.Saved;
            }
        }

        /// <inheritdoc />
        public async Task AutosaveDocument(Document document, IEditorContract editor)
        {
            await File.WriteAllTextAsync(document.AutosavePath, editor.GetContent());
            document.Status = DocumentStatus.AutoSaved;
        }

        /// <inheritdoc />
        public async Task SaveDocument(Document document, IEditorContract editor)
        {
            await File.WriteAllTextAsync(document.Path, editor.GetContent());
            document.Status = DocumentStatus.Saved;

            if (File.Exists(document.AutosavePath)) File.Delete(document.AutosavePath);
        }
    }
}