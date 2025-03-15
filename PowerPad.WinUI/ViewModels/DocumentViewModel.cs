using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models;
using PowerPad.Core.Services;
using System;
using System.Windows.Input;

namespace PowerPad.WinUI.ViewModels
{
    public partial class DocumentViewModel : ObservableObject
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
            _documentService = Ioc.Default.GetRequiredService<IDocumentService>();
            _editorControl = editorControl;

            _documentService.LoadDocument(_document, _editorControl);
            _lastSaveTime = DateTime.Now;

            SaveCommand = new RelayCommand(Save);
            AutosaveCommand = new RelayCommand(Autosave);
            RenameCommand = new RelayCommand<string>(Rename);
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

            var workspaceService = Ioc.Default.GetRequiredService<IWorkspaceService>();

            workspaceService.RenameDocument(_document, newName);

            OnPropertyChanged(nameof(Name));
        }
    }
}