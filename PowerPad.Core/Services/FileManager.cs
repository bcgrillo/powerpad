using PowerPad.Core.Models;

namespace PowerPad.Core.Services
{
    public class FileManager
    {
        private readonly string workspacePath;
        private readonly string hiddenFolder;

        public FileManager(string workspacePath)
        {
            this.workspacePath = workspacePath;
            this.hiddenFolder = Path.Combine(workspacePath, ".powerpad");

            EnsureDirectories();
        }

        private void EnsureDirectories()
        {
            if (!Directory.Exists(hiddenFolder))
                Directory.CreateDirectory(hiddenFolder);
        }

        public List<FileItem> GetFilesAndFoldersHierarchy()
        {
            return GetFilesAndFoldersRecursive(workspacePath);
        }

        private List<FileItem> GetFilesAndFoldersRecursive(string path)
        {
            List<FileItem> items = new();
            foreach (var dir in Directory.GetDirectories(path))
            {
                if (!dir.EndsWith(".powerpad"))
                {
                    var folderItem = new FileItem { Name = Path.GetFileName(dir), IsFolder = true, Children = GetFilesAndFoldersRecursive(dir) };
                    folderItem.Glyph = "\uE8D5";
                    items.Add(folderItem);
                }
            }
            foreach (var file in Directory.GetFiles(path))
            {
                if (!file.StartsWith(hiddenFolder) && !file.EndsWith(".autosave"))
                {
                    var fileItem = new FileItem { Name = Path.GetFileNameWithoutExtension(file), Path = Path.GetFullPath(file), IsFolder = false, Type = GetFileType(file) };
                    fileItem.Glyph = GetFileGlyph(fileItem.Type);
                    items.Add(fileItem);
                }
            }
            return items;
        }

        private FileType GetFileType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return extension switch
            {
                ".txt" => FileType.Text,
                ".md" => FileType.Markdown,
                ".todo" => FileType.TodoList,
                ".chat" => FileType.Chat,
                ".cs" => FileType.Code,
                ".js" => FileType.Code,
                ".html" => FileType.Code,
                ".css" => FileType.Code,
                ".java" => FileType.Code,
                ".py" => FileType.Code,
                ".cpp" => FileType.Code,
                ".c" => FileType.Code,
                ".h" => FileType.Code,
                ".htm" => FileType.Code,
                _ => FileType.Unknown
            };
        }

        private string GetFileGlyph(FileType fileType)
        {
            return fileType switch
            {
                FileType.Text => "\uE8A5",
                FileType.Markdown => "\uE8A5",
                FileType.TodoList => "\uE946",
                FileType.Chat => "\uE717",
                FileType.Code => "\uE943",
                _ => "\uE8A5"
            };
        }
    }
}
