using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.WinUI.ViewModels
{
    public enum DocumentTypes
    {
        Text,
        Chat,
        Markdown,
        ToDo,
        Search
    }

    public static class DocumentTypesExtensions
    {
        public static string ToFileExtension(this DocumentTypes type)
        {
            return type switch
            {
                DocumentTypes.Text => ".txt",
                DocumentTypes.Chat => ".chat",
                DocumentTypes.Markdown => ".md",
                DocumentTypes.ToDo => ".todo",
                DocumentTypes.Search => ".search",
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }

        public static string ToGlyph(this DocumentTypes type)
        {
            return type switch
            {
                DocumentTypes.Text => "\uE70B",
                DocumentTypes.Chat => "\uE15F",
                DocumentTypes.Markdown => "\uE8A5",
                DocumentTypes.ToDo => "\uF0E3",
                DocumentTypes.Search => "\uF6FA",
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }
    }
}
