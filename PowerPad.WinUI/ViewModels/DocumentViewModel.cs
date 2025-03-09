using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Data;
using PowerPad.Core.Models;
using PowerPad.Core.Services;
using PowerPad.WinUI.Components.Editors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            get => _document.Status;
            set
            {
                if (_document.Status != value)
                {
                    _document.Status = value;
                    OnPropertyChanged(nameof(Status));
                    SaveCommand.NotifyCanExecuteChanged();
                }
            }
        }

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

            SaveCommand = new RelayCommand(Save, CanSave);
            AutosaveCommand = new RelayCommand(Autosave);
            RenameCommand = new RelayCommand<string>(Rename);
        }

        private bool CanSave() => Status != DocumentStatus.Saved;

        private void Save()
        {
            _documentService.SaveDocument(_document, _editorControl);
            _lastSaveTime = DateTime.Now;
            SaveCommand.NotifyCanExecuteChanged();
        }

        private void Autosave()
        {
            _documentService.AutosaveDocument(_document, _editorControl);
            _lastSaveTime = DateTime.Now;
        }

        private void Rename(string? newName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(newName, nameof(newName));

            _documentService.RenameDocument(_document, _editorControl, newName);
            OnPropertyChanged(nameof(Name));
        }
    }
}
