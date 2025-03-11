using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OllamaSharp.Models.Chat;
using PowerPad.Core.Models;
using PowerPad.Core.Services;
using System;

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

            Root = new FolderEntryViewModel(_workspaceService.OpenWorkspace(@"D:\OneDrive\Escritorio\Universidad\PruebasTFG"), null);

            MoveEntryCommand = new RelayCommand<MoveEntryParameters>(MoveEntry);

            NewEntryCommand = new RelayCommand<NewEntryParameters>(NewEntry);
        }

        private void MoveEntry(MoveEntryParameters parameters)
        {
            parameters.NewParent ??= Root;

            if (parameters.Entry.Type == EntryType.Document)
            {
                var document = (Document)parameters.Entry.ModelEntry;
                var targetFolder = (Folder)parameters.NewParent.ModelEntry;
                _workspaceService.MoveDocument(document, targetFolder);
            }
            else
            {
                var folder = (Folder)parameters.Entry.ModelEntry;
                var targetFolder = (Folder)parameters.NewParent.ModelEntry;
                _workspaceService.MoveFolder(folder, targetFolder);
            }
        }

        private void NewEntry(NewEntryParameters parameters)
        {
            ArgumentException.ThrowIfNullOrEmpty(parameters.Name, nameof(parameters.Name));

            FolderEntryViewModel parent = parameters.Parent ?? Root;

            if (parameters.Type == EntryType.Folder)
            {
                var folderModel = (Folder)parent.ModelEntry;

                var newFolderModel = new Folder(parameters.Name);

                _workspaceService.CreateFolder(folderModel, newFolderModel);

                parent.Children!.Add(new FolderEntryViewModel(newFolderModel, parent));
            }
            else
            {
                var folderModel = (Folder)parent.ModelEntry;

                var newDocumentModel = new Document(parameters.Name, parameters.DocumentType!.Value.ToFileExtension());

                _workspaceService.CreateDocument(folderModel, newDocumentModel);

                parent.Children!.Add(new FolderEntryViewModel(newDocumentModel, parent));
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

    public class MoveEntryParameters
    {
        public FolderEntryViewModel Entry { get; set; }
        public FolderEntryViewModel? NewParent { get; set; }
    }
}
