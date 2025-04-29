using System;

namespace PowerPad.WinUI.ViewModels.FileSystem
{
    public enum DocumentType
    {
        Note,
        Chat,
    }

    public static class DocumentTypesExtensions
    {
        public static string ToFileExtension(this DocumentType type)
        {
            return type switch
            {
                DocumentType.Note => ".txt",
                DocumentType.Chat => ".chat",
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }

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
