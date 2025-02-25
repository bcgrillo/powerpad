using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Data;
using PowerPad.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.WinUI.ViewModels
{
    public partial class TextContent : ObservableObject
    {
        [ObservableProperty]
        private string? _content;

        [ObservableProperty]
        private FileStatus? _status;

        public IRelayCommand SaveCommand { get; }

        public TextContent()
        {
            SaveCommand = new RelayCommand(Save, CanSave);
        }

        private bool CanSave() => Status == FileStatus.Dirty;

        private void Save()
        {
            // Lógica para guardar el contenido
            Debug.WriteLine($"Guardado");
        }

        partial void OnContentChanged(string oldValue, string newValue)
        {
            Status = FileStatus.Dirty;
            SaveCommand.NotifyCanExecuteChanged(); // Actualiza CanExecute cuando cambia el contenido
        }
    }
}
