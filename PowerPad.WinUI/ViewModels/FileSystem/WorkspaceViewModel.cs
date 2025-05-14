using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PowerPad.Core.Models.FileSystem;
using PowerPad.Core.Services.Config;
using PowerPad.Core.Services.FileSystem;
using PowerPad.WinUI.Helpers;
using PowerPad.WinUI.Messages;
using System;
using System.Collections.ObjectModel;
using static PowerPad.WinUI.Configuration.ConfigConstants;

namespace PowerPad.WinUI.ViewModels.FileSystem
{
    /// <summary>
    /// ViewModel for managing the workspace, including folders, documents, and recently opened workspaces.
    /// </summary>
    public partial class WorkspaceViewModel : ObservableObject
    {
        private const int MAX_RECENTLY_WORKSPACES = 5;

        private readonly IWorkspaceService _workspaceService;
        private readonly IConfigStore _appConfigStore;

        /// <summary>
        /// Gets or sets the root folder entry of the workspace.
        /// </summary>
        [ObservableProperty]
        public partial FolderEntryViewModel Root { get; set; }

        /// <summary>
        /// Gets the collection of recently opened workspaces.
        /// </summary>
        public ObservableCollection<string> RecentlyWorkspaces { get; }

        /// <summary>
        /// Command to open a workspace.
        /// </summary>
        public IRelayCommand OpenWorkspaceCommand { get; }

        /// <summary>
        /// Command to move an entry (folder or document) within the workspace.
        /// </summary>
        public IRelayCommand MoveEntryCommand { get; }

        /// <summary>
        /// Command to create a new entry (folder or document) in the workspace.
        /// </summary>
        public IRelayCommand NewEntryCommand { get; }

        /// <summary>
        /// Gets or sets the path of the currently opened document.
        /// </summary>
        [ObservableProperty]
        public partial string? CurrentDocumentPath { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkspaceViewModel"/> class.
        /// </summary>
        public WorkspaceViewModel()
        {
            _workspaceService = App.Get<IWorkspaceService>();
            _appConfigStore = App.Get<IConfigStore>();

            Root = new(_workspaceService.Root, null);

            MoveEntryCommand = new RelayCommand<MoveEntryParameters>(MoveEntry);

            NewEntryCommand = new RelayCommand<NewEntryParameters>(NewEntry);

            OpenWorkspaceCommand = new RelayCommand<string>(OpenWorkspace);

            RecentlyWorkspaces = _appConfigStore.Get<ObservableCollection<string>>(StoreKey.RecentlyWorkspaces);
            CurrentDocumentPath = _appConfigStore.TryGet<string>(StoreKey.CurrentDocumentPath);
        }

        /// <summary>
        /// Updates the configuration store when the current document path changes.
        /// </summary>
        /// <param name="oldValue">The previous document path.</param>
        /// <param name="newValue">The new document path.</param>
        partial void OnCurrentDocumentPathChanged(string? oldValue, string? newValue)
        {
            _appConfigStore.Set(StoreKey.CurrentDocumentPath, newValue);
        }

        /// <summary>
        /// Moves an entry (folder or document) to a new parent or position.
        /// </summary>
        /// <param name="parameters">The parameters specifying the entry to move, its new parent, and position.</param>
        private void MoveEntry(MoveEntryParameters? parameters)
        {
            ArgumentNullException.ThrowIfNull(parameters);

            var newParent = parameters.NewParent ?? Root;
            var targetPosition = newParent.Children!.IndexOf(parameters.Entry);

            if (newParent.ModelEntry == parameters.Entry.ModelEntry.Parent)
            {
                if (parameters.Entry.Type == EntryType.Document)
                {
                    var document = (Document)parameters.Entry.ModelEntry;
                    _workspaceService.SetPosition(document, targetPosition);
                }
                else
                {
                    var folder = (Folder)parameters.Entry.ModelEntry;
                    _workspaceService.SetPosition(folder, targetPosition);
                }
            }
            else
            {
                var originalName = parameters.Entry.Name;

                if (parameters.Entry.Type == EntryType.Document)
                {
                    var document = (Document)parameters.Entry.ModelEntry;
                    var targetFolder = (Folder)newParent.ModelEntry;
                    _workspaceService.MoveDocument(document, targetFolder, targetPosition);
                }
                else
                {
                    var folder = (Folder)parameters.Entry.ModelEntry;
                    var targetFolder = (Folder)newParent.ModelEntry;
                    _workspaceService.MoveFolder(folder, targetFolder, targetPosition);
                }

                if (originalName != parameters.Entry.Name) parameters.Entry.NameChanged();
            }
        }

        /// <summary>
        /// Creates a new entry (folder or document) in the workspace.
        /// </summary>
        /// <param name="parameters">The parameters specifying the type, name, and parent of the new entry.</param>
        private void NewEntry(NewEntryParameters? parameters)
        {
            ArgumentNullException.ThrowIfNull(parameters);

            FolderEntryViewModel parent = parameters.Parent ?? Root;

            FolderEntryViewModel createdEntry;
            if (parameters.Type == EntryType.Folder)
            {
                var folderModel = (Folder)parent.ModelEntry;

                var newFolderModel = new Folder(parameters.Name);

                _workspaceService.CreateFolder(folderModel, newFolderModel);

                createdEntry = new(newFolderModel, parent);
                parent.Children!.Add(createdEntry);
            }
            else
            {
                var folderModel = (Folder)parent.ModelEntry;

                var newDocumentModel = new Document(parameters.Name, parameters.DocumentType!.Value.ToFileExtension());

                _workspaceService.CreateDocument(folderModel, newDocumentModel, parameters.Content);

                createdEntry = new(newDocumentModel, parent);
                parent.Children!.Add(createdEntry);
            }

            WeakReferenceMessenger.Default.Send(new FolderEntryCreated(createdEntry));
        }

        /// <summary>
        /// Opens a workspace by loading its root folder and updating the recently opened workspaces list.
        /// </summary>
        /// <param name="path">The path of the workspace to open.</param>
        private void OpenWorkspace(string? path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);

            _workspaceService.OpenWorkspace(path);

            Root = new(_workspaceService.Root, null);

            RecentlyWorkspaces.Remove(path);
            RecentlyWorkspaces.Insert(0, path);

            if (RecentlyWorkspaces.Count > MAX_RECENTLY_WORKSPACES)
            {
                RecentlyWorkspaces.RemoveAt(5);
            }

            _appConfigStore.Set(StoreKey.RecentlyWorkspaces, RecentlyWorkspaces);
        }
    }

    /// <summary>
    /// Parameters for creating a new entry (folder or document).
    /// </summary>
    public class NewEntryParameters
    {
        /// <summary>
        /// Gets the parent folder where the new entry will be created.
        /// </summary>
        public FolderEntryViewModel? Parent { get; private set; }

        /// <summary>
        /// Gets the type of the new entry (folder or document).
        /// </summary>
        public EntryType Type { get; private set; }

        /// <summary>
        /// Gets the document type, if the new entry is a document.
        /// </summary>
        public DocumentType? DocumentType { get; private set; }

        /// <summary>
        /// Gets the name of the new entry.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the content of the new document, if applicable.
        /// </summary>
        public string? Content { get; private set; }

        /// <summary>
        /// Creates parameters for a new document.
        /// </summary>
        /// <param name="parent">The parent folder where the document will be created.</param>
        /// <param name="documentType">The type of the document.</param>
        /// <param name="name">The name of the document. If null, a default name will be generated.</param>
        /// <param name="content">The content of the document.</param>
        /// <returns>A new instance of <see cref="NewEntryParameters"/> for a document.</returns>
        public static NewEntryParameters NewDocument(FolderEntryViewModel? parent, DocumentType documentType, string? name = null, string? content = null)
        {
            return new(name ?? NameGeneratorHelper.NewDocumentName(documentType))
            {
                Parent = parent,
                Type = EntryType.Document,
                DocumentType = documentType,
                Content = content,
            };
        }

        /// <summary>
        /// Creates parameters for a new folder.
        /// </summary>
        /// <param name="parent">The parent folder where the folder will be created.</param>
        /// <param name="name">The name of the folder. If null, a default name will be generated.</param>
        /// <returns>A new instance of <see cref="NewEntryParameters"/> for a folder.</returns>
        public static NewEntryParameters NewFolder(FolderEntryViewModel? parent, string? name = null)
        {
            return new(name ?? NameGeneratorHelper.NewFolderName())
            {
                Parent = parent,
                Type = EntryType.Folder,
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewEntryParameters"/> class with the specified name.
        /// </summary>
        /// <param name="name">The name to associate with the new entry. This value cannot be null or empty.</param>
        private NewEntryParameters(string name) { Name = name; }
    }

    /// <summary>
    /// Parameters for moving an entry (folder or document) within the workspace.
    /// </summary>
    /// <param name="Entry">The entry to move.</param>
    /// <param name="NewParent">The new parent folder for the entry. If null, the root folder is used.</param>
    public record MoveEntryParameters(FolderEntryViewModel Entry, FolderEntryViewModel? NewParent);
}
