
using PowerPad.Core.Models;
using PowerPad.WinUI.Components.Editors;

namespace PowerPad.Core.Services
{
    public interface IDocumentService
    {
        void LoadDocument(Document document, IEditorControl output);
        void AutosaveDocument(Document document, IEditorControl control);
        void SaveDocument(Document document, IEditorControl control);
        void RenameDocument(Document document, string newName);
    }

    public class DocumentService : IDocumentService
    {
        public void LoadDocument(Document document, IEditorControl control)
        {
            var autosaveExists = File.Exists(AutosavePath(document.Path));

            if (autosaveExists)
            {
                control.SetContent(File.ReadAllText(AutosavePath(document.Path)));
                document.Status = DocumentStatus.AutoSaved;
            }
            else
            {
                control.SetContent(File.ReadAllText(document.Path));
                document.Status = DocumentStatus.Saved;
            }
        }

        public void AutosaveDocument(Document document, IEditorControl control)
        {
            File.WriteAllText(AutosavePath(document.Path), control.GetContent());
            document.Status = DocumentStatus.AutoSaved;
        }

        public void SaveDocument(Document document, IEditorControl control)
        {
            File.WriteAllText(document.Path, control.GetContent());
            document.Status = DocumentStatus.Saved;

            if (File.Exists(AutosavePath(document.Path)))
            {
                File.Delete(AutosavePath(document.Path));
            }
        }

        public void RenameDocument(Document document, string newName)
        {
            var directory = Path.GetDirectoryName(document.Path)!;
            var extension = Path.GetExtension(document.Path);
            var newPath = Path.Combine(directory, newName + extension);

            File.Move(document.Path, newPath);
            document.Path = newPath;
        }

        private string AutosavePath(string path) => path + ".autosave";
    }
}
