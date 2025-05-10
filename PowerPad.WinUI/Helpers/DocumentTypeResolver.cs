using PowerPad.WinUI.ViewModels.FileSystem;
using System.Linq;

namespace PowerPad.WinUI.Helpers
{
    public static class DocumentTypeResolver
    {
        public static DocumentType FromFilePath(string filePath)
        {
            string extension = filePath.Split('.')[^1].ToLower();

            return extension switch
            {
                "txt" => DocumentType.Note,
                "chat" => DocumentType.Chat,
                _ => DocumentType.Note
            };
        }
    }
}