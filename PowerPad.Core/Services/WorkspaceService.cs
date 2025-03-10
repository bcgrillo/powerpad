using PowerPad.Core.Models;
using System.Collections.ObjectModel;

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
    }

    public class WorkspaceService : IWorkspaceService
    {
        public const string AUTO_SAVE_EXTENSION = ".autosave";

        public Folder OpenWorkspace(string path)
        {
            //var hiddenFolder = Path.Combine(path, ".powerpad");

            //if (!Directory.Exists(hiddenFolder)) Directory.CreateDirectory(hiddenFolder);

            var root = new Folder(path);
            root.AddFolders(GetFoldersRecursive(path, root));
            root.AddDocuments(GetDocuments(path));

            return root;
        }

        private IEnumerable<Folder> GetFoldersRecursive(string path, Folder parent)
        {
            Collection<Folder> folders = [];
            foreach (var directory in Directory.GetDirectories(path))
            {
                var folder = new Folder(directory);
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
                    documents.Add(new Document(file));
            }
            return documents;
        }

        public void MoveDocument(Document document, Folder targetFolder)
        {
            var newPath = Path.Combine(targetFolder.Path, Path.GetFileName(document.Path));
            File.Move(document.Path, newPath);
            document.Path = newPath;

            if (File.Exists(AutosavePath(document.Path)))
            {
                newPath = Path.Combine(targetFolder.Path, Path.GetFileName(AutosavePath(document.Path)));
                File.Move(AutosavePath(document.Path), newPath);
            }

            document.Parent?.RemoveDocument(document);
            targetFolder.AddDocument(document);
        }

        public void MoveFolder(Folder folder, Folder targetFolder)
        {
            var newPath = Path.Combine(targetFolder.Path, Path.GetFileName(folder.Path));
            Directory.Move(folder.Path, newPath);

            folder.Parent?.RemoveFolder(folder);
            targetFolder.AddFolder(folder);
        }

        public void CreateDocument(Folder parent, Document document)
        {
            if (File.Exists(document.Path)) throw new InvalidOperationException("Document already exists");

            File.WriteAllText(document.Path, string.Empty);

            parent.AddDocument(document);
        }

        public void CreateFolder(Folder parent, Folder folder)
        {
            if (Directory.Exists(folder.Path)) throw new InvalidOperationException("Folder already exists");

            Directory.CreateDirectory(folder.Path);

            parent.AddFolder(folder);
        }

        public void DeleteDocument(Document document)
        {
            File.Delete(document.Path);
            if (File.Exists(AutosavePath(document.Path))) File.Delete(AutosavePath(document.Path));

            document.Parent?.RemoveDocument(document);
        }

        public void DeleteFolder(Folder folder)
        {
            Directory.Delete(folder.Path, true);

            folder.Parent?.RemoveFolder(folder);
        }

        private string AutosavePath(string path) => path + AUTO_SAVE_EXTENSION;
    }
}
