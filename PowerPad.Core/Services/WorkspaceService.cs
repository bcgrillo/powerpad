using PowerPad.Core.Models;
using System.Collections.ObjectModel;

namespace PowerPad.Core.Services
{
    public interface IWorkspaceService
    {
        Folder OpenWorkspace(string path);

        void MoveDocument(Document document, Folder targetFolder);

        void MoveFolder(Folder folder, Folder targetFolder);
    }

    public class WorkspaceService : IWorkspaceService
    {
        public const string AUTO_SAVE_EXTENSION = ".autosave";

        public Folder OpenWorkspace(string path)
        {
            //var hiddenFolder = Path.Combine(path, ".powerpad");

            //if (!Directory.Exists(hiddenFolder)) Directory.CreateDirectory(hiddenFolder);

            var root = new Folder(path, null);
            root.Folders = GetFoldersRecursive(path, root);
            root.Documents = GetDocuments(path, root);

            return root;
        }

        private Collection<Folder> GetFoldersRecursive(string path, IFolderEntry? parent)
        {
            Collection<Folder> folders = [];
            foreach (var directory in Directory.GetDirectories(path))
            {
                var folder = new Folder(directory, parent);
                folder.Folders = GetFoldersRecursive(directory, folder);
                folder.Documents = GetDocuments(directory, folder);
                folders.Add(folder);
            }
            return folders;
        }

        private Collection<Document>GetDocuments(string path, IFolderEntry parent)
        {
            Collection<Document> documents = new();
            foreach (var file in Directory.GetFiles(path))
            {
                if (!file.EndsWith(AUTO_SAVE_EXTENSION))
                    documents.Add(new Document(file, parent));
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

            ((Folder)document.Parent!).Documents!.Remove(document);
            targetFolder.Documents!.Add(document);
            document.Parent = targetFolder;
        }

        public void MoveFolder(Folder folder, Folder targetFolder)
        {
            var newPath = Path.Combine(targetFolder.Path, Path.GetFileName(folder.Path));
            Directory.Move(folder.Path, newPath);

            ((Folder)folder.Parent!).Folders!.Remove(folder);
            targetFolder.Folders!.Add(folder);
            folder.Parent = targetFolder;
        }

        public void CreateDocument(Folder parent, Document document)
        {
        }

        private string AutosavePath(string path) => path + AUTO_SAVE_EXTENSION;
    }
}
