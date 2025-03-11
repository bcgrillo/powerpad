using PowerPad.Core.Models;
using System.Collections.ObjectModel;
using static PowerPad.Core.Services.AutosaveConventions;

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
    }

    public class WorkspaceService : IWorkspaceService
    {
        public Folder OpenWorkspace(string path)
        {
            //var hiddenFolder = Path.Combine(path, ".powerpad");

            //if (!Directory.Exists(hiddenFolder)) Directory.CreateDirectory(hiddenFolder);

            var root = Folder.CreateRoot(path);
            root.AddFolders(GetFoldersRecursive(path, root));
            root.AddDocuments(GetDocuments(path));

            return root;
        }

        private IEnumerable<Folder> GetFoldersRecursive(string path, Folder parent)
        {
            Collection<Folder> folders = [];
            foreach (var directory in Directory.GetDirectories(path))
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

            if (File.Exists(newPath)) throw new InvalidOperationException("Document already exists");

            File.WriteAllText(newPath, string.Empty);

            parent.AddDocument(newDocument);
        }

        public void CreateFolder(Folder parent, Folder newFolder)
        {
            var newPath = Path.Combine(parent.Path, newFolder.Name);

            if (Directory.Exists(newPath)) throw new InvalidOperationException("Folder already exists");

            Directory.CreateDirectory(newPath);

            parent.AddFolder(newFolder);
        }

        public void DeleteDocument(Document document)
        {
            File.Delete(document.Path);

            if (File.Exists(document.AutosavePath)) File.Delete(document.AutosavePath);

            document.Parent!.RemoveDocument(document);
        }

        public void DeleteFolder(Folder folder)
        {
            Directory.Delete(folder.Path, true);

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
    }
}
