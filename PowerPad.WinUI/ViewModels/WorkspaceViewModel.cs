using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using OllamaSharp.Models.Chat;
using PowerPad.Core.Configuration;
using PowerPad.Core.Models;
using PowerPad.Core.Services;
using PowerPad.WinUI.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using static PowerPad.WinUI.Configuration.ConfigConstants;

namespace PowerPad.WinUI.ViewModels
{
    public partial class WorkspaceViewModel : ObservableObject
    {
        private readonly IWorkspaceService _workspaceService;
        private readonly IConfigStore _appConfigStore;

        [ObservableProperty]
        private FolderEntryViewModel _root;

        [ObservableProperty]
        private ObservableCollection<string> _recentlyWorkspaces;

        public IRelayCommand MoveEntryCommand { get; }

        public IRelayCommand NewEntryCommand { get; }

        public WorkspaceViewModel()
        {
            _workspaceService = Ioc.Default.GetRequiredService<IWorkspaceService>();
            _appConfigStore = Ioc.Default.GetRequiredService<IConfigStore>();

            Root = new FolderEntryViewModel(_workspaceService.Root, null);

            MoveEntryCommand = new RelayCommand<MoveEntryParameters>(MoveEntry);

            NewEntryCommand = new RelayCommand<NewEntryParameters>(NewEntry);

            var recentlyWorkspaces = _appConfigStore.Get<ObservableCollection<string>>(Keys.RecentlyWorkspaces);

            _recentlyWorkspaces = [.. recentlyWorkspaces];
        }

        private void MoveEntry(MoveEntryParameters? parameters)
        {
            ArgumentNullException.ThrowIfNull(parameters, nameof(parameters));

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

        private void NewEntry(NewEntryParameters? parameters)
        {
            ArgumentNullException.ThrowIfNull(parameters, nameof(parameters));

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
        public string Name { get; set; }

        private NewEntryParameters(string name) { Name = name; }

        public static NewEntryParameters NewDocument(FolderEntryViewModel? parent, DocumentTypes documentType, string? name = null)
        {
            return new NewEntryParameters(name ?? NewEntryNameHelper.NewDocumentName(documentType))
            {
                Parent = parent,
                Type = EntryType.Document,
                DocumentType = documentType,
            };
        }

        public static NewEntryParameters NewFolder(FolderEntryViewModel? parent, string? name = null)
        {
            return new NewEntryParameters(name ?? NewEntryNameHelper.NewFolderName())
            {
                Parent = parent,
                Type = EntryType.Folder,
            };
        }
    }

    public class MoveEntryParameters(FolderEntryViewModel entry, FolderEntryViewModel? newParent)
    {
        public FolderEntryViewModel Entry { get; set; } = entry;
        public FolderEntryViewModel? NewParent { get; set; } = newParent;
    }
}
