using PowerPad.WinUI.ViewModels.FileSystem;
using System.Linq;

namespace PowerPad.WinUI.Helpers
{
    /// <summary>
    /// Provides functionality to resolve the document type based on the file path.
    /// </summary>
    public static class DocumentTypeResolver
    {
        /// <summary>
        /// Determines the <see cref="DocumentType"/> from the given file path.
        /// </summary>
        /// <param name="filePath">The file path to analyze.</param>
        /// <returns>The resolved <see cref="DocumentType"/> based on the file extension.</returns>
        public static DocumentType FromFilePath(string filePath)
        {
            // Extract the file extension from the file path and convert it to lowercase.
            string extension = filePath.Split('.')[^1].ToLower();

            // Return the corresponding DocumentType based on the file extension.
            return extension switch
            {
                "txt" => DocumentType.Note,
                "chat" => DocumentType.Chat,
                _ => DocumentType.Note
            };
        }
    }
}