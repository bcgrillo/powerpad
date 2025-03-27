using PowerPad.Core.Contracts;
using PowerPad.Core.Models;
using System.Collections.ObjectModel;
using static PowerPad.Core.Services.Conventions;

namespace PowerPad.Core.Services
{
    public interface IWorkspaceService
    {
        Folder Root { get; }

        void MoveDocument(Document document, Folder targetFolder, int targetPosition);

        void MoveFolder(Folder folder, Folder targetFolder, int targetPosition);

        void SetPosition(Document entry, int targetPosition);

        void SetPosition(Folder folder, int targetPosition);

        void CreateDocument(Folder parent, Document document);

        void CreateFolder(Folder parent, Folder folder);

        void DeleteDocument(Document document);

        void DeleteFolder(Folder folder);

        void RenameDocument(Document document, string newName);

        void RenameFolder(Folder folder, string newName);

        void OpenWorkspace(string rootFolder);
    }

    public class WorkspaceService : IWorkspaceService
    {
        private Folder _root;
        private string _configFolder;
        private string _trashFolder;
        private readonly IConfigStoreService _configStoreService;
        private readonly IOrderService _orderService;

        public Folder Root => _root;

        public WorkspaceService(string rootFolder, IConfigStoreService configStoreService, IOrderService orderService)
        {
            _configStoreService = configStoreService;
            _orderService = orderService;

            _configFolder = Path.Combine(rootFolder, CONFIG_FOLDER_NAME);
            _trashFolder = Path.Combine(rootFolder, TRASH_FOLDER_NAME);
            _root = InitializeRootFolder(rootFolder);
        }

        private Folder InitializeRootFolder(string rootFolder)
        {
            if (!Directory.Exists(_configFolder)) Directory.CreateDirectory(_configFolder);
            if (!Directory.Exists(_trashFolder)) Directory.CreateDirectory(_trashFolder);

            var folder = Folder.CreateRoot(rootFolder);
            folder.AddFolders(GetFoldersRecursive(rootFolder));
            folder.AddDocuments(GetDocuments(rootFolder));

            _orderService.LoadOrderRecursive(folder);

            return folder;
        }

        private Collection<Folder> GetFoldersRecursive(string path)
        {
            Collection<Folder> folders = [];

            var directories = Directory.GetDirectories(path)
                .Where(d => d != _configFolder && d != _trashFolder);

            foreach (var directory in directories)
            {
                var folder = new Folder(Path.GetFileName(directory));
                folder.AddFolders(GetFoldersRecursive(directory));
                folder.AddDocuments(GetDocuments(directory));
                folders.Add(folder);
            }
            return folders;
        }

        private static Collection<Document>GetDocuments(string directory)
        {
            Collection<Document> documents = [];
            foreach (var file in Directory.GetFiles(directory))
            {
                var filename = Path.GetFileNameWithoutExtension(file);
                var extension = Path.GetExtension(file);

                if (extension != AUTO_SAVE_EXTENSION && extension != ORDER_FILE_NAME)
                {
                    documents.Add(new Document(filename, extension));
                }
            }
            return documents;
        }

        public void MoveDocument(Document document, Folder targetFolder, int targetPosition)
        {
            var sourceFolder = document.Parent!;
            var newPath = $"{targetFolder.Path}\\{document.Name}{document.Extension}";

            File.Move(document.Path, newPath);

            if (File.Exists(document.AutosavePath)) File.Move(document.AutosavePath, AutosavePath(newPath));

            document.Parent!.RemoveDocument(document);

            targetFolder.AddDocument(document);

            _orderService.UpdateOrderAfterMove(sourceFolder, targetFolder, $"{document.Name}{document.Extension}", targetPosition);
        }

        public void MoveFolder(Folder folder, Folder targetFolder, int targetPosition)
        {
            var sourceFolder = folder.Parent!;
            var newPath = Path.Combine(targetFolder.Path, folder.Name);

            Directory.Move(folder.Path, newPath);

            folder.Parent!.RemoveFolder(folder);

            targetFolder.AddFolder(folder);

            _orderService.UpdateOrderAfterMove(sourceFolder, targetFolder, folder.Name, targetPosition);
        }

        public void SetPosition(Document document, int targetPosition)
        {
            _orderService.UpdateOrderAfterMove(document.Parent!, null, $"{document.Name}{document.Extension}", targetPosition);
        }

        public void SetPosition(Folder folder, int targetPosition)
        {
            _orderService.UpdateOrderAfterMove(folder.Parent!, null, folder.Name, targetPosition);
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

            _orderService.UpdateOrderAfterCreation(parent, $"{newDocument.Name}{newDocument.Extension}");
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

            _orderService.UpdateOrderAfterCreation(parent, newFolder.Name);

        }

        public void DeleteDocument(Document document)
        {
            var sourceFolder = document.Parent!;
            var newPath = $"{_trashFolder}\\{document.Name}{document.Extension}";
            var originalName = document.Name;

            int counter = 1;
            while (File.Exists(newPath))
            {
                var altName = $"{originalName} ({counter})";
                newPath = $"{_trashFolder}\\{altName}{document.Extension}";
                counter++;
            }

            File.Move(document.Path, newPath);

            if (File.Exists(document.AutosavePath)) File.Move(document.AutosavePath, AutosavePath(newPath), true);

            document.Parent!.RemoveDocument(document);

            _orderService.UpdateOrderAfterDeletion(sourceFolder, $"{originalName}{document.Extension}");
        }

        public void DeleteFolder(Folder folder)
        {
            var sourceFolder = folder.Parent!;
            var newPath = Path.Combine(_trashFolder, folder.Name);
            var originalName = folder.Name;

            int counter = 1;
            while (Directory.Exists(newPath))
            {
                var altName = $"{originalName} ({counter})";
                newPath = $"{_trashFolder}\\{altName}";
                counter++;
            }

            Directory.Move(folder.Path, newPath);

            folder.Parent!.RemoveFolder(folder);

            _orderService.UpdateOrderAfterDeletion(sourceFolder, originalName);
        }

        public void RenameDocument(Document document, string newName)
        {
            var oldName = $"{document.Name}{document.Extension}";
            var newPath = $"{document.Parent!.Path}\\{newName}{document.Extension}";

            File.Move(document.Path, newPath);

            if (File.Exists(document.AutosavePath)) File.Move(document.AutosavePath, AutosavePath(newPath));

            document.Name = newName;

            _orderService.UpdateOrderAfterRename(document.Parent, oldName, $"{document.Name}{document.Extension}");
        }

        public void RenameFolder(Folder folder, string newName)
        {
            var oldName = folder.Name;
            var newPath = Path.Combine(folder.Parent!.Path, newName);

            Directory.Move(folder.Path, newPath);

            folder.Name = newName;

            _orderService.UpdateOrderAfterRename(folder.Parent, oldName, folder.Name);
        }

        public void OpenWorkspace(string rootFolder)
        {
            _configFolder = Path.Combine(rootFolder, CONFIG_FOLDER_NAME);
            _trashFolder = Path.Combine(rootFolder, TRASH_FOLDER_NAME);
            _root = InitializeRootFolder(rootFolder);
        }

        private IConfigStore GetConfigStore() => _configStoreService.GetConfigStore(_configFolder);
    }
}
