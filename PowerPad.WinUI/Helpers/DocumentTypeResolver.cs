using PowerPad.WinUI.ViewModels;
using System.Linq;

namespace PowerPad.WinUI.Helpers
{
    public static class DocumentTypeResolver
    {
        public static DocumentTypes FromFilePath(string filePath)
        {
            string extension = filePath.Split('.').Last().ToLower();

            return extension switch
            {
                "txt" => DocumentTypes.Text,
                "chat" => DocumentTypes.Chat,
                "md" => DocumentTypes.Markdown,
                "todo" => DocumentTypes.ToDo,
                "search" => DocumentTypes.Search,
                _ => DocumentTypes.Text
            };
        }
    }
}