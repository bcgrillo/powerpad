using PowerPad.Core.Contracts;
using PowerPad.Core.Models.FileSystem;

namespace PowerPad.Core.Services.FileSystem
{
    public interface IDocumentService
    {
        void LoadDocument(Document document, IEditorContract output);
        void AutosaveDocument(Document document, IEditorContract control);
        void SaveDocument(Document document, IEditorContract control);
    }

    public class DocumentService : IDocumentService
    {
        public void LoadDocument(Document document, IEditorContract control)
        {
            var autosaveExists = File.Exists(document.AutosavePath);

            if (autosaveExists)
            {
                control.SetContent(File.ReadAllText(document.AutosavePath));
                document.Status = DocumentStatus.AutoSaved;
            }
            else
            {
                control.SetContent(File.ReadAllText(document.Path));
                document.Status = DocumentStatus.Saved;
            }
        }

        public void AutosaveDocument(Document document, IEditorContract control)
        {
            File.WriteAllText(document.AutosavePath, control.GetContent());
            document.Status = DocumentStatus.AutoSaved;
        }

        public void SaveDocument(Document document, IEditorContract control)
        {
            File.WriteAllText(document.Path, control.GetContent());
            document.Status = DocumentStatus.Saved;

            if (File.Exists(document.AutosavePath)) File.Delete(document.AutosavePath);
        }
    }
}