using PowerPad.Core.Models;

namespace PowerPad.Core.Models
{
    public class FileItem
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public bool IsFolder { get; set; }
        public FileType Type { get; set; } = FileType.Unknown;
        public FileStatus Status { get; set; } = FileStatus.Unloaded;
        public List<FileItem>? Children { get; set; }
        public string Glyph { get; set; } = string.Empty;

        public string AutoSavePath => Path + ".autosave";

        private string? _content;

        public string Content
        { 
            get
            {
                if (Status == FileStatus.Unloaded) Load();
                return _content!;
            }
            set
            {
                _content = value;
                Status = FileStatus.Dirty;
            }
        }

        public void AutoSave()
        {
            File.WriteAllText(AutoSavePath, Content);
            Status = FileStatus.AutoSaved;
        }

        public void Save()
        {
            File.WriteAllText(Path, Content);
            if (File.Exists(AutoSavePath)) File.Delete(AutoSavePath);
            Status = FileStatus.Saved;
        }

        private void Load()
        {
            if (File.Exists(AutoSavePath))
            {
                _content = File.ReadAllText(AutoSavePath);
                Status = FileStatus.AutoSaved;
            }
            else
            {
                _content = File.ReadAllText(Path);
                Status = FileStatus.Saved;
            }
        }
    }
}
