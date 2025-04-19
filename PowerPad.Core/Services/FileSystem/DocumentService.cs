using PowerPad.Core.Contracts;
using PowerPad.Core.Models.FileSystem;

namespace PowerPad.Core.Services.FileSystem
{
    public interface IDocumentService
    {
        void LoadDocument(Document document, IEditorContract output);
        Task AutosaveDocument(Document document, IEditorContract control);
        Task SaveDocument(Document document, IEditorContract control);
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

        public async Task AutosaveDocument(Document document, IEditorContract control)
        {
            await File.WriteAllTextAsync(document.AutosavePath, control.GetContent());
            document.Status = DocumentStatus.AutoSaved;
        }

        public async Task SaveDocument(Document document, IEditorContract control)
        {
            await File.WriteAllTextAsync(document.Path, control.GetContent());
            document.Status = DocumentStatus.Saved;

            if (File.Exists(document.AutosavePath)) File.Delete(document.AutosavePath);
        }
    }
}