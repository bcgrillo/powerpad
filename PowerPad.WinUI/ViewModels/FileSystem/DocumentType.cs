using System;

namespace PowerPad.WinUI.ViewModels.FileSystem
{
    public enum DocumentType
    {
        Text,
        Chat,
        /*Markdown,
        ToDo,
        Search*/
    }

    public static class DocumentTypesExtensions
    {
        public static string ToFileExtension(this DocumentType type)
        {
            return type switch
            {
                DocumentType.Text => ".txt",
                DocumentType.Chat => ".chat",
                /*DocumentType.Markdown => ".md",
                DocumentType.ToDo => ".todo",
                DocumentType.Search => ".search",*/
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }

        public static string ToGlyph(this DocumentType type)
        {
            return type switch
            {
                DocumentType.Text => "\uE70B",
                DocumentType.Chat => "\uE15F",
                /*DocumentType.Markdown => "\uE8A5",
                DocumentType.ToDo => "\uF0E3",
                DocumentType.Search => "\uF6FA",*/
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }
    }
}
