using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.FileSystem;
using PowerPad.Core.Services.FileSystem;
using PowerPad.WinUI.Helpers;
using PowerPad.WinUI.Messages;
using System;
using System.Threading.Tasks;

namespace PowerPad.WinUI.ViewModels.FileSystem
{
    /// <summary>
    /// ViewModel for managing a document, including its status, saving, renaming, and name generation.
    /// </summary>
    public partial class DocumentViewModel : ObservableObject, IRecipient<FolderEntryChanged>
    {
        private const int MIN_WORDS_GENERATE_NAME = 50;
        private const int SAMPLE_LENGHT_GENERATE_NAME = 500;

        private readonly IDocumentService _documentService;
        private readonly Document _document;
        private readonly IEditorContract _editorControl;
        private DateTime _lastSaveTime;
        private bool _untitled;

        /// <summary>
        /// Gets the name of the document.
        /// </summary>
        public string Name { get => _document.Name; }

        /// <summary>
        /// Gets or sets the status of the document.
        /// </summary>
        public DocumentStatus Status
        {
            get => _document.Status!.Value;
            set
            {
                if (_document.Status != value)
                {
                    _document.Status = value;

                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the document can be saved.
        /// </summary>
        public bool CanSave => Status != DocumentStatus.Saved;

        /// <summary>
        /// Gets the last save time of the document.
        /// </summary>
        public DateTime LastSaveTime { get => _lastSaveTime; }

        /// <summary>
        /// Gets or sets the previous content of the document.
        /// </summary>
        [ObservableProperty]
        public partial string? PreviousContent { get; set; }

        /// <summary>
        /// Gets or sets the next content of the document.
        /// </summary>
        [ObservableProperty]
        public partial string? NextContent { get; set; }

        /// <summary>
        /// Command to save the document.
        /// </summary>
        public IAsyncRelayCommand SaveCommand { get; }

        /// <summary>
        /// Command to autosave the document.
        /// </summary>
        public IAsyncRelayCommand AutosaveCommand { get; }

        /// <summary>
        /// Command to rename the document.
        /// </summary>
        public IRelayCommand RenameCommand { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentViewModel"/> class.
        /// </summary>
        /// <param name="document">The document to manage.</param>
        /// <param name="editorControl">The editor control associated with the document.</param>
        public DocumentViewModel(Document document, IEditorContract editorControl)
        {
            _document = document;
            _documentService = App.Get<IDocumentService>();
            _editorControl = editorControl;

            _documentService.LoadDocument(_document, _editorControl);
            _lastSaveTime = DateTime.UtcNow;

            SaveCommand = new AsyncRelayCommand(Save);
            AutosaveCommand = new AsyncRelayCommand(Autosave);
            RenameCommand = new RelayCommand<string>(Rename);

            _untitled = NameGeneratorHelper.CheckNewNamePattern(document.Name);

            WeakReferenceMessenger.Default.Register(this);
        }


        /// <summary>
        /// Notifies that the document's name has changed.
        /// </summary>
        public void NameChanged()
        {
            WeakReferenceMessenger.Default.Send(new FolderEntryChanged(_document) { NameChanged = true });

            OnPropertyChanged(nameof(Name));
        }

        /// <summary>
        /// Handles the receipt of a <see cref="FolderEntryChanged"/> message.
        /// </summary>
        /// <param name="message">The message indicating a folder entry change.</param>
        public void Receive(FolderEntryChanged message)
        {
            if (message.Value == _document && message.NameChanged)
            {
                _untitled = false;
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// Saves the document and updates its status.
        /// </summary>
        private async Task Save()
        {
            await _documentService.SaveDocument(_document, _editorControl);
            _lastSaveTime = DateTime.UtcNow;

            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(CanSave));

            if (_untitled && _editorControl.WordCount() >= MIN_WORDS_GENERATE_NAME) await GenerateName();
        }

        /// <summary>
        /// Autosaves the document and updates its status.
        /// </summary>
        private async Task Autosave()
        {
            await _documentService.AutosaveDocument(_document, _editorControl);
            _lastSaveTime = DateTime.UtcNow;

            OnPropertyChanged(nameof(Status));

            if (_untitled && _editorControl.WordCount() >= MIN_WORDS_GENERATE_NAME) await GenerateName();
        }

        /// <summary>
        /// Renames the document to a new name.
        /// </summary>
        /// <param name="newName">The new name for the document.</param>
        private void Rename(string? newName)
        {
            _untitled = false;

            ArgumentException.ThrowIfNullOrWhiteSpace(newName);

            var workspaceService = App.Get<IWorkspaceService>();

            workspaceService.RenameDocument(_document, newName);

            NameChanged();
        }

        /// <summary>
        /// Generates a new name for the document based on its content.
        /// </summary>
        private async Task GenerateName()
        {
            _untitled = false;

            var content = _editorControl.GetContent(plainText: true);
            var sampleContent = content[..Math.Min(content.Length, SAMPLE_LENGHT_GENERATE_NAME)];

            var generatedName = await NameGeneratorHelper.Generate(sampleContent);

            if (!string.IsNullOrEmpty(generatedName))
            {
                var workspaceService = App.Get<IWorkspaceService>();

                workspaceService.RenameDocument(_document, generatedName);

                NameChanged();
            }
        }
    }
}