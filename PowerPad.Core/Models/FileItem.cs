using PowerPad.Core.Models;

namespace PowerPad.Core.Models
{
    public class FileItem
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public bool IsFolder { get; set; }
        public FileType Type { get; set; } = FileType.Unknown;
        public List<FileItem>? Children { get; set; }
        public string Glyph { get; set; } = string.Empty;
    }
}
