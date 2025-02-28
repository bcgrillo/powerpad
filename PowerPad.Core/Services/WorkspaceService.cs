using PowerPad.Core.Models;
using System.Collections.ObjectModel;

namespace PowerPad.Core.Services
{
    public interface IWorkspaceService
    {
        Folder OpenWorkspace(string path);
    }

    public class WorkspaceService : IWorkspaceService
    {
        public Folder OpenWorkspace(string path)
        {
            //var hiddenFolder = Path.Combine(path, ".powerpad");

            //if (!Directory.Exists(hiddenFolder)) Directory.CreateDirectory(hiddenFolder);

            return new Folder(path, GetFoldersRecursive(path), GetDocuments(path));
        }

        private List<Folder> GetFoldersRecursive(string path)
        {
            List<Folder> folders = [];
            foreach (var directory in Directory.GetDirectories(path))
            {
                folders.Add(new Folder(directory, GetFoldersRecursive(directory), GetDocuments(directory)));
            }
            return folders;
        }

        private List<Document>GetDocuments(string path)
        {
            List<Document> documents = new();
            foreach (var file in Directory.GetFiles(path))
            {
                documents.Add(new Document(file));
            }
            return documents;
        }
    }
}
