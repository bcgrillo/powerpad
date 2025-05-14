using System;

namespace PowerPad.WinUI.ViewModels.FileSystem
{
    /// <summary>
    /// Represents the type of a document in the application.
    /// </summary>
    public enum DocumentType
    {
        /// <summary>Note document.</summary>
        Note,
        /// <summary>Represents a chat document.</summary>
        Chat,
    }

    /// <summary>
    /// Provides extension methods for the <see cref="DocumentType"/> enumeration.
    /// </summary>
    public static class DocumentTypesExtensions
    {
        /// <summary>
        /// Converts a <see cref="DocumentType"/> to its corresponding file extension.
        /// </summary>
        /// <param name="type">The document type to convert.</param>
        /// <returns>The file extension associated with the document type.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the document type is not recognized.</exception>
        public static string ToFileExtension(this DocumentType type)
        {
            return type switch
            {
                DocumentType.Note => ".txt",
                DocumentType.Chat => ".chat",
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }

        /// <summary>
        /// Converts a <see cref="DocumentType"/> to its corresponding glyph representation.
        /// </summary>
        /// <param name="type">The document type to convert.</param>
        /// <returns>The glyph associated with the document type.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the document type is not recognized.</exception>
        public static string ToGlyph(this DocumentType type)
        {
            return type switch
            {
                DocumentType.Note => "\uE70B",
                DocumentType.Chat => "\uE15F",
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }
    }
}
