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

        private void NewEntry((FolderEntryViewModel? parent, EntryType type, DocumentTypes? documentType, string name) parameters)
        {
            IFolderEntry newEntry;
            FolderEntryViewModel parent = parameters.parent ?? Root;
            var newPath = parent.ModelEntry.Path + "\\" + parameters.name;

            if (parameters.type == EntryType.Folder)
            {
                newEntry = new Folder(newPath, (parameters.parent ?? Root).ModelEntry);

                parent.Children.Add(new FolderEntryViewModel((Folder)newEntry));
            }
            else
            {
                switch(parameters.documentType)
                {
                    case DocumentTypes.Markdown:
                        newPath += ".md";
                        break;
                    case DocumentTypes.ToDo:
                        newPath += ".todo";
                        break;
                    case DocumentTypes.Search:
                        newPath += ".search";
                        break;
                    case DocumentTypes.Chat:
                        newPath += ".chat";
                        break;
                    default:
                        newPath += ".txt";
                        break;
                }

                newEntry = new Document(newPath, (parameters.parent ?? Root).ModelEntry);

                parent.Children.Add(new FolderEntryViewModel((Document)newEntry));
            }
        }
    }
}
