using PowerPad.WinUI.ViewModels.FileSystem;
using System.Linq;

namespace PowerPad.WinUI.Helpers
{
    public static class DocumentTypeResolver
    {
        public static DocumentType FromFilePath(string filePath)
        {
            string extension = filePath.Split('.').Last().ToLower();

            return extension switch
            {
                "txt" => DocumentType.Text,
                "chat" => DocumentType.Chat,
                "md" => DocumentType.Markdown,
                "todo" => DocumentType.ToDo,
                "search" => DocumentType.Search,
                _ => DocumentType.Text
            };
        }
    }
}