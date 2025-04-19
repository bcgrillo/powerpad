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
    public partial class DocumentViewModel : ObservableObject, IRecipient<FolderEntryChanged>
    {
        private const int MIN_WORDS_GENERATE_NAME = 30;
        private const int SAMPLE_LENGHT_GENERATE_NAME = 500;

        private readonly IDocumentService _documentService;
        private readonly Document _document;
        private readonly IEditorContract _editorControl;
        private DateTime _lastSaveTime;
        private bool _untitled;

        public string Name { get => _document.Name; }

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

        public bool CanSave => Status != DocumentStatus.Saved;

        public DateTime LastSaveTime { get => _lastSaveTime; }

        public IAsyncRelayCommand SaveCommand { get; }

        public IAsyncRelayCommand AutosaveCommand { get; }

        public IRelayCommand RenameCommand { get; }

        [ObservableProperty]
        private string? _previousContent;

        [ObservableProperty]
        private string? _nextContent;

        public DocumentViewModel(Document document, IEditorContract editorControl)
        {
            _document = document;
            _documentService = App.Get<IDocumentService>();
            _editorControl = editorControl;

            _documentService.LoadDocument(_document, _editorControl);
            _lastSaveTime = DateTime.Now;

            SaveCommand = new AsyncRelayCommand(Save);
            AutosaveCommand = new AsyncRelayCommand(Autosave);
            RenameCommand = new RelayCommand<string>(Rename);

            _untitled = NameGeneratorHelper.CheckNewNamePattern(document.Name);

            WeakReferenceMessenger.Default.Register(this);
        }

        private async Task Save()
        {
            if (_untitled && _editorControl.WordCount() >= MIN_WORDS_GENERATE_NAME) await GenerateName();

            await _documentService.SaveDocument(_document, _editorControl);
            _lastSaveTime = DateTime.Now;

            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(CanSave));
        }

        private async Task Autosave()
        {
            if (_untitled && _editorControl.WordCount() >= MIN_WORDS_GENERATE_NAME) await GenerateName();

            await _documentService.AutosaveDocument(_document, _editorControl);
            _lastSaveTime = DateTime.Now;

            OnPropertyChanged(nameof(Status));
        }

        private void Rename(string? newName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(newName, nameof(newName));

            var workspaceService = App.Get<IWorkspaceService>();

            workspaceService.RenameDocument(_document, newName);

            NameChanged();
        }

        private async Task GenerateName()
        {
            var content = _editorControl.GetContent(plainText: true);
            var sampleContent = content[..Math.Min(content.Length, SAMPLE_LENGHT_GENERATE_NAME)];

            var generatedName = await NameGeneratorHelper.Generate(sampleContent);

            if (generatedName != null) Rename(generatedName);
        }

        public void NameChanged()
        {
            WeakReferenceMessenger.Default.Send(new FolderEntryChanged(_document) { NameChanged = true});

            OnPropertyChanged(nameof(Name));
            _untitled = false;
        }

        public void Receive(FolderEntryChanged message)
        {
            if (message.Value == _document)
            {
                if (message.NameChanged) OnPropertyChanged(nameof(Name));
                _untitled = false;
            }
        }
    }
}