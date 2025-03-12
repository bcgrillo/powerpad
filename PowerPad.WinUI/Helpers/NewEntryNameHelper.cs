using PowerPad.WinUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Helpers
{
    public static class NewEntryNameHelper
    {
        public static string NewFolderName() => "Nueva carpeta";

        public static string NewDocumentName(DocumentTypes type)
        {
            return type switch
            {
                DocumentTypes.Text => "Nueva nota",
                DocumentTypes.Chat => "Nuevo chat",
                DocumentTypes.Markdown => "Nuevo Markdown",
                DocumentTypes.ToDo => "Nueva lista",
                DocumentTypes.Search => "Nueva búsqueda",
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }
    }
}
