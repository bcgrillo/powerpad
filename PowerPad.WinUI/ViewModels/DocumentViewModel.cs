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
        private readonly IEditorControl _editorControl;

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

        public IRelayCommand SaveCommand { get; }

        public DocumentViewModel(Document document, IEditorControl editorControl)
        {
            _document = document;
            _documentService = Ioc.Default.GetRequiredService<IDocumentService>();
            _editorControl = editorControl;

            _documentService.LoadDocument(_document, _editorControl);

            SaveCommand = new RelayCommand(Save, CanSave);
        }

        private bool CanSave() => Status == DocumentStatus.Dirty;

        private void Save()
        {
            _documentService.SaveDocument(_document, _editorControl);
        }
    }
}
