using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.FileSystem;
using PowerPad.Core.Services.FileSystem;
using PowerPad.WinUI.Messages;
using System;

namespace PowerPad.WinUI.ViewModels.FileSystem
{
    public partial class DocumentViewModel : ObservableObject, IRecipient<FolderEntryChanged>
    {
        private readonly IDocumentService _documentService;
        private readonly Document _document;
        private readonly IEditorContract _editorControl;
        private DateTime _lastSaveTime;

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

        public IRelayCommand SaveCommand { get; }

        public IRelayCommand AutosaveCommand { get; }

        public IRelayCommand RenameCommand { get; }

        public DocumentViewModel(Document document, IEditorContract editorControl)
        {
            _document = document;
            _documentService = App.Get<IDocumentService>();
            _editorControl = editorControl;

            _documentService.LoadDocument(_document, _editorControl);
            _lastSaveTime = DateTime.Now;

            SaveCommand = new RelayCommand(Save);
            AutosaveCommand = new RelayCommand(Autosave);
            RenameCommand = new RelayCommand<string>(Rename);

            WeakReferenceMessenger.Default.Register(this);
        }

        private void Save()
        {
            _documentService.SaveDocument(_document, _editorControl);
            _lastSaveTime = DateTime.Now;

            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(CanSave));
        }

        private void Autosave()
        {
            _documentService.AutosaveDocument(_document, _editorControl);
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

        public void NameChanged()
        {
            WeakReferenceMessenger.Default.Send(new FolderEntryChanged(_document) { NameChanged = true});

            OnPropertyChanged(nameof(Name));
        }

        public void Receive(FolderEntryChanged message)
        {
            if (message.Value == _document)
            {
                if (message.NameChanged) OnPropertyChanged(nameof(Name));
            }
        }
    }
}