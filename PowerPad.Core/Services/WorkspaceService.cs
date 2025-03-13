using PowerPad.Core.Models;
using System.Collections.ObjectModel;
using System.Text.Json;
using static PowerPad.Core.Services.AutosaveConventions;
using Timer = System.Timers.Timer;

namespace PowerPad.Core.Services
{
    public interface IWorkspaceService
    {
        Folder OpenWorkspace(string path);

        void MoveDocument(Document document, Folder targetFolder);

        void MoveFolder(Folder folder, Folder targetFolder);

        void CreateDocument(Folder parent, Document document);

        void CreateFolder(Folder parent, Folder folder);

        void DeleteDocument(Document document);

        void DeleteFolder(Folder folder);

        void RenameDocument(Document document, string newName);

        void RenameFolder(Folder folder, string newName);

        void SaveConfig<T>(string key, T config);

        T? GetConfig<T>(string key);
    }

    public class WorkspaceService : IWorkspaceService
    {
        private const string CONFIG_FOLDER_NAME = ".powerpad";
        private const string TRASH_FOLDER_NAME = ".trash";

        private string _configFolder;
        private string _trashFolder;
        private Dictionary<string, (object? value, bool dirty)> _configStore = [];
        private Timer _timer;

        public WorkspaceService()
        {
            AppDomain.CurrentDomain.ProcessExit += (sender, e) => StoreConfig();

            _timer = new Timer(2000);
            _timer.Elapsed += (sender, e) => StoreConfig();
            _timer.Start();
        }

        public Folder OpenWorkspace(string path)
        {
            _configFolder = Path.Combine(path, CONFIG_FOLDER_NAME);
            _trashFolder = Path.Combine(path, TRASH_FOLDER_NAME);

            if (!Directory.Exists(_configFolder)) Directory.CreateDirectory(_configFolder);
            if (!Directory.Exists(_trashFolder)) Directory.CreateDirectory(_trashFolder);

            var root = Folder.CreateRoot(path);
            root.AddFolders(GetFoldersRecursive(path, root));
            root.AddDocuments(GetDocuments(path));

            return root;
        }

        private IEnumerable<Folder> GetFoldersRecursive(string path, Folder parent)
        {
            Collection<Folder> folders = [];

            var directories = Directory.GetDirectories(path)
                .Where(d => d != _configFolder && d != _trashFolder);

            foreach (var directory in directories)
            {
                var folder = new Folder(Path.GetFileName(directory));
                folder.AddFolders(GetFoldersRecursive(directory, folder));
                folder.AddDocuments(GetDocuments(directory));
                folders.Add(folder);
            }
            return folders;
        }

        private IEnumerable<Document>GetDocuments(string directory)
        {
            Collection<Document> documents = new();
            foreach (var file in Directory.GetFiles(directory))
            {
                if (!file.EndsWith(AUTO_SAVE_EXTENSION))
                    documents.Add(new Document(Path.GetFileNameWithoutExtension(file), Path.GetExtension(file)));
            }
            return documents;
        }

        public void MoveDocument(Document document, Folder targetFolder)
        {
            var newPath = $"{targetFolder.Path}\\{document.Name}{document.Extension}";

            File.Move(document.Path, newPath);

            if (File.Exists(document.AutosavePath)) File.Move(document.AutosavePath, AutosavePath(newPath));

            document.Parent!.RemoveDocument(document);

            targetFolder.AddDocument(document);
        }

        public void MoveFolder(Folder folder, Folder targetFolder)
        {
            var newPath = Path.Combine(targetFolder.Path, Path.GetFileName(folder.Path));
            Directory.Move(folder.Path, newPath);

            folder.Parent!.RemoveFolder(folder);
            targetFolder.AddFolder(folder);
        }

        public void CreateDocument(Folder parent, Document newDocument)
        {
            var newPath = $"{parent.Path}\\{newDocument.Name}{newDocument.Extension}";
            var originalName = newDocument.Name;

            int counter = 1;
            while (File.Exists(newPath))
            {
                newDocument.Name = $"{originalName} ({counter})";
                newPath = $"{parent.Path}\\{newDocument.Name}{newDocument.Extension}";
                counter++;
            }

            File.WriteAllText(newPath, string.Empty);

            parent.AddDocument(newDocument);
        }

        public void CreateFolder(Folder parent, Folder newFolder)
        {
            var newPath = Path.Combine(parent.Path, newFolder.Name);
            var originalName = newFolder.Name;

            int counter = 1;
            while (Directory.Exists(newPath))
            {
                newFolder.Name = $"{originalName} ({counter})";
                newPath = Path.Combine(parent.Path, newFolder.Name);
                counter++;
            }

            Directory.CreateDirectory(newPath);

            parent.AddFolder(newFolder);
        }

        public void DeleteDocument(Document document)
        {
            var newPath = $"{_trashFolder}\\{document.Name}{document.Extension}";

            File.Move(document.Path, newPath);

            if (File.Exists(document.AutosavePath)) File.Move(document.AutosavePath, AutosavePath(newPath));

            document.Parent!.RemoveDocument(document);
        }

        public void DeleteFolder(Folder folder)
        {
            var newPath = Path.Combine(_trashFolder, Path.GetFileName(folder.Path));
            Directory.Move(folder.Path, newPath);

            folder.Parent!.RemoveFolder(folder);
        }

        public void RenameDocument(Document document, string newName)
        {
            var newPath = $"{document.Parent!.Path}\\{newName}{document.Extension}";

            File.Move(document.Path, newPath);

            if (File.Exists(document.AutosavePath)) File.Move(document.AutosavePath, AutosavePath(newPath));

            document.Name = newName;
        }

        public void RenameFolder(Folder folder, string newName)
        {
            var newPath = Path.Combine(folder.Parent!.Path, newName);

            Directory.Move(folder.Path, newPath);

            folder.Name = newName;
        }

        public void SaveConfig<T>(string key, T config)
        {
            _configStore[key] = (config, true);
        }

        public T? GetConfig<T>(string key)
        {
            if (_configStore.TryGetValue(key, out var config))
            {
                return (T?)config.value;
            }
            return default;
        }

        private void StoreConfig()
        {
            for(var i = 0; i < _configStore.Count; i++)
            {
                var (key, value) = _configStore.ElementAt(i);
                if (value.dirty)
                {
                    var path = Path.Combine(_configFolder, $"{key}.json");
                    var jsonConfig = JsonSerializer.Serialize(value.value);
                    File.WriteAllText(path, jsonConfig);
                    value.dirty = false;
                }
            }
        }
    }
}
