using PowerPad.WinUI.ViewModels.FileSystem;
using System;

namespace PowerPad.WinUI.Helpers
{
    public static class NewEntryNameHelper
    {
        public static string NewFolderName() => "Nueva carpeta";

        public static string NewDocumentName(DocumentType type)
        {
            return type switch
            {
                DocumentType.Text => "Nueva nota",
                DocumentType.Chat => "Nuevo chat",
                DocumentType.Markdown => "Nuevo Markdown",
                DocumentType.ToDo => "Nueva lista",
                DocumentType.Search => "Nueva búsqueda",
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }
    }
}
