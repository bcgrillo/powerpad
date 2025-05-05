using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PowerPad.WinUI.Components.Editors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PowerPad.WinUI.ViewModels.FileSystem
{
    public partial class DraftDocumentViewModel : ObservableObject
    {
        public bool CanSave => true;

        [ObservableProperty]
        public partial string? Content { get; set; }

        [ObservableProperty]
        public partial string? PreviousContent { get; set; }

        [ObservableProperty]
        public partial string? NextContent { get; set; }

        public IAsyncRelayCommand SaveCommand { get; }

        public IAsyncRelayCommand AutosaveCommand { get; }

        public DraftDocumentViewModel()
        {
            SaveCommand = new AsyncRelayCommand(Save);
            AutosaveCommand = new AsyncRelayCommand(Autosave);
        }

        private async Task Save()
        {

        }

        private async Task Autosave()
        {

        }
    }
}
