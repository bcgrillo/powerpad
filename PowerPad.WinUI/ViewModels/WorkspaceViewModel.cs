using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PowerPad.Core.Models;
using PowerPad.Core.Services;

namespace PowerPad.WinUI.ViewModels
{
    public partial class WorkspaceViewModel : ObservableObject
    {
        private readonly IWorkspaceService _workspaceService;

        [ObservableProperty]
        private FolderEntryViewModel _root;

        public IRelayCommand MoveEntryCommand { get; }

        public IRelayCommand NewEntryCommand { get; }

        public WorkspaceViewModel(IWorkspaceService workspaceService)
        {
            _workspaceService = workspaceService;

            Root = new FolderEntryViewModel(_workspaceService.OpenWorkspace(@"D:\OneDrive\Escritorio\Universidad\PruebasTFG"));

            MoveEntryCommand = new RelayCommand<(FolderEntryViewModel, FolderEntryViewModel?)>(MoveEntry);

            NewEntryCommand = new RelayCommand<NewEntryParameters>(NewEntry);
        }

        private void MoveEntry((FolderEntryViewModel entry, FolderEntryViewModel? newParent) parameters)
        {
            parameters.newParent ??= Root;

            if (parameters.entry.Type == EntryType.Document)
            {
                var document = (Document)parameters.entry.ModelEntry;
                var targetFolder = (Folder)parameters.newParent.ModelEntry;
                _workspaceService.MoveDocument(document, targetFolder);
            }
            else
            {
                var folder = (Folder)parameters.entry.ModelEntry;
                var targetFolder = (Folder)parameters.newParent.ModelEntry;
                _workspaceService.MoveFolder(folder, targetFolder);
            }
        }

        private void NewEntry(NewEntryParameters parameters)
        {
            FolderEntryViewModel parent = parameters.Parent ?? Root;
            var newPath = parent.ModelEntry.Path + "\\" + parameters.Name;

            if (parameters.Type == EntryType.Folder)
            {
                var folderModel = (Folder)parent.ModelEntry;

                var newFolderModel = new Folder(newPath);

                _workspaceService.CreateFolder(folderModel, newFolderModel);

                parent.Children.Add(new FolderEntryViewModel(newFolderModel));
            }
            else
            {
                newPath += parameters.DocumentType!.Value.ToFileExtension();

                var folderModel = (Folder)parent.ModelEntry;
                var newDocumentModel = new Document(newPath);

                _workspaceService.CreateDocument(folderModel, newDocumentModel);

                parent.Children.Add(new FolderEntryViewModel(newDocumentModel));
            }
        }
    }

    public class NewEntryParameters
    {
        public FolderEntryViewModel? Parent { get; set; }
        public EntryType Type { get; set; }
        public DocumentTypes? DocumentType { get; set; }
        public string? Name { get; set; }

        private NewEntryParameters() { }

        public static NewEntryParameters NewDocument(FolderEntryViewModel? parent, DocumentTypes documentType, string name)
        {
            return new NewEntryParameters
            {
                Parent = parent,
                Type = EntryType.Document,
                DocumentType = documentType,
                Name = name
            };
        }

        public static NewEntryParameters NewFolder(FolderEntryViewModel? parent, string name)
        {
            return new NewEntryParameters
            {
                Parent = parent,
                Type = EntryType.Folder,
                Name = name
            };
        }
    }
}
