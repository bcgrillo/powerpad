using PowerPad.Core.Models.FileSystem;
using System.Collections.ObjectModel;
using static PowerPad.Core.Services.Conventions;

namespace PowerPad.Core.Services.FileSystem
{
    /// <summary>
    /// Provides methods to manage and manipulate the workspace, including folders and documents.
    /// </summary>
    public interface IWorkspaceService
    {
        /// <summary>
        /// Gets the root folder of the workspace.
        /// </summary>
        Folder Root { get; }

        /// <summary>
        /// Moves a document to a target folder at a specified position.
        /// </summary>
        void MoveDocument(Document document, Folder targetFolder, int targetPosition);

        /// <summary>
        /// Moves a folder to a target folder at a specified position.
        /// </summary>
        void MoveFolder(Folder folder, Folder targetFolder, int targetPosition);

        /// <summary>
        /// Sets the position of a document within its parent folder.
        /// </summary>
        void SetPosition(Document document, int targetPosition);

        /// <summary>
        /// Sets the position of a folder within its parent folder.
        /// </summary>
        void SetPosition(Folder folder, int targetPosition);

        /// <summary>
        /// Creates a new document in the specified parent folder.
        /// </summary>
        void CreateDocument(Folder parent, Document newDocument, string? content = null);

        /// <summary>
        /// Creates a new folder in the specified parent folder.
        /// </summary>
        void CreateFolder(Folder parent, Folder newFolder);

        /// <summary>
        /// Deletes a document and moves it to the trash folder.
        /// </summary>
        void DeleteDocument(Document document);

        /// <summary>
        /// Deletes a folder and moves it to the trash folder.
        /// </summary>
        void DeleteFolder(Folder folder);

        /// <summary>
        /// Renames a document to a new name.
        /// </summary>
        void RenameDocument(Document document, string newName);

        /// <summary>
        /// Renames a folder to a new name.
        /// </summary>
        void RenameFolder(Folder folder, string newName);

        /// <summary>
        /// Opens a workspace by initializing the root folder.
        /// </summary>
        void OpenWorkspace(string rootFolder);
    }

    /// <summary>
    /// Implementation of <see cref="IWorkspaceService"/> to manage workspace operations.
    /// </summary>
    public class WorkspaceService : IWorkspaceService
    {
        private Folder _root;
        private string _trashFolder;
        private readonly IOrderService _orderService;

        /// <inheritdoc />
        public Folder Root => _root;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkspaceService"/> class.
        /// </summary>
        /// <param name="rootFolder">The root folder of the workspace.</param>
        /// <param name="orderService">The service to manage folder and document order.</param>
        public WorkspaceService(string rootFolder, IOrderService orderService)
        {
            _orderService = orderService;

            _trashFolder = Path.Combine(rootFolder, TRASH_FOLDER_NAME);
            _root = InitializeRootFolder(rootFolder);
        }

        /// <inheritdoc />
        public void MoveDocument(Document document, Folder targetFolder, int targetPosition)
        {
            var originalFullName = $"{document.Name}{document.Extension}";
            var originalPath = document.Path;
            var originalAutosavePath = document.AutosavePath;
            var sourceFolder = document.Parent!;

            var newPath = GetAvailableNewPath(targetFolder, document);

            File.Move(originalPath, newPath);

            if (File.Exists(originalAutosavePath)) File.Move(originalAutosavePath, AutosavePath(newPath));

            document.Parent!.RemoveDocument(document);

            targetFolder.AddDocument(document);

            _orderService.UpdateOrderAfterMove(sourceFolder, targetFolder, originalFullName, targetPosition);
        }

        /// <inheritdoc />
        public void MoveFolder(Folder folder, Folder targetFolder, int targetPosition)
        {
            var originalName = folder.Name;
            var originalPath = folder.Path;
            var sourceFolder = folder.Parent!;

            var newPath = GetAvailableNewPath(targetFolder, folder);

            Directory.Move(originalPath, newPath);

            folder.Parent!.RemoveFolder(folder);

            targetFolder.AddFolder(folder);

            _orderService.UpdateOrderAfterMove(sourceFolder, targetFolder, originalName, targetPosition);
        }

        /// <inheritdoc />
        public void SetPosition(Document document, int targetPosition)
        {
            _orderService.UpdateOrderAfterMove(document.Parent!, null, $"{document.Name}{document.Extension}", targetPosition);
        }

        /// <inheritdoc />
        public void SetPosition(Folder folder, int targetPosition)
        {
            _orderService.UpdateOrderAfterMove(folder.Parent!, null, folder.Name, targetPosition);
        }

        /// <inheritdoc />
        public void CreateDocument(Folder parent, Document newDocument, string? content = null)
        {
            string newPath = GetAvailableNewPath(parent, newDocument);

            File.WriteAllText(newPath, content ?? string.Empty);

            parent.AddDocument(newDocument);

            _orderService.UpdateOrderAfterCreation(parent, $"{newDocument.Name}{newDocument.Extension}");
        }

        /// <inheritdoc />
        public void CreateFolder(Folder parent, Folder newFolder)
        {
            string newPath = GetAvailableNewPath(parent, newFolder);

            Directory.CreateDirectory(newPath);

            parent.AddFolder(newFolder);

            _orderService.UpdateOrderAfterCreation(parent, newFolder.Name);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public void RenameDocument(Document document, string newName)
        {
            var oldFullName = $"{document.Name}{document.Extension}";
            var oldPath = document.Path;
            var oldAutosavePath = document.AutosavePath;

            var newPath = GetAvailableNewPath(document.Parent!, document, newName);

            File.Move(oldPath, newPath);

            if (File.Exists(oldAutosavePath)) File.Move(oldAutosavePath, AutosavePath(newPath));

            _orderService.UpdateOrderAfterRename(document.Parent!, oldFullName, $"{document.Name}{document.Extension}");
        }

        /// <inheritdoc />
        public void RenameFolder(Folder folder, string newName)
        {
            var oldName = folder.Name;
            var oldPath = folder.Path;

            var newPath = GetAvailableNewPath(folder.Parent!, folder, newName);

            Directory.Move(oldPath, newPath);

            _orderService.UpdateOrderAfterRename(folder.Parent!, oldName, folder.Name);
        }

        /// <inheritdoc />
        public void OpenWorkspace(string rootFolder)
        {
            _trashFolder = Path.Combine(rootFolder, TRASH_FOLDER_NAME);
            _root = InitializeRootFolder(rootFolder);
        }

        /// <summary>
        /// Initializes the root folder and loads its contents recursively.
        /// </summary>
        private Root InitializeRootFolder(string rootFolder)
        {
            if (!Directory.Exists(_trashFolder)) Directory.CreateDirectory(_trashFolder);

            var root = new Root(rootFolder);
            root.AddFolders(GetFoldersRecursive(rootFolder));
            root.AddDocuments(GetDocuments(rootFolder));

            _orderService.LoadOrderRecursive(root);

            return root;
        }

        /// <summary>
        /// Recursively retrieves all subfolders from the specified path.
        /// </summary>
        private Collection<Folder> GetFoldersRecursive(string path)
        {
            Collection<Folder> folders = [];

            var directories = Directory.GetDirectories(path).Where(d => d != _trashFolder);

            foreach (var directory in directories)
            {
                var folder = new Folder(Path.GetFileName(directory));
                folder.AddFolders(GetFoldersRecursive(directory));
                folder.AddDocuments(GetDocuments(directory));
                folders.Add(folder);
            }
            return folders;
        }

        /// <summary>
        /// Retrieves all documents from the specified directory.
        /// </summary>
        private static Collection<Document> GetDocuments(string directory)
        {
            Collection<Document> documents = [];
            foreach (var file in Directory.GetFiles(directory))
            {
                var filename = Path.GetFileNameWithoutExtension(file);
                var extension = Path.GetExtension(file);

                if (extension != AUTO_SAVE_EXTENSION && extension != ORDER_FILE_NAME)
                {
                    documents.Add(new(filename, extension));
                }
            }
            return documents;
        }

        /// <summary>
        /// Gets a new available path for a document, ensuring no name conflicts.
        /// </summary>
        private static string GetAvailableNewPath(Folder parent, Document child, string? newName = null)
        {
            if (newName is not null) child.Name = newName;

            var newPath = $"{parent.Path}\\{child.Name}{child.Extension}";
            var originalName = child.Name;

            int counter = 1;
            while (File.Exists(newPath))
            {
                child.Name = $"{originalName} ({++counter})";
                newPath = $"{parent.Path}\\{child.Name}{child.Extension}";
            }

            return newPath;
        }

        /// <summary>
        /// Gets a new available path for a folder, ensuring no name conflicts.
        /// </summary>
        private static string GetAvailableNewPath(Folder parent, Folder child, string? newName = null)
        {
            if (newName is not null) child.Name = newName;

            var newPath = Path.Combine(parent.Path, child.Name);
            var originalName = child.Name;

            int counter = 1;
            while (Directory.Exists(newPath))
            {
                child.Name = $"{originalName} ({++counter})";
                newPath = Path.Combine(parent.Path, child.Name);
            }

            return newPath;
        }
    }
}