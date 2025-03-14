using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OllamaSharp.Models;
using PowerPad.Core.Models;
using PowerPad.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.WinUI.ViewModels
{
    public partial class OllamaViewModel : AIServiceViewModel
    {
        private readonly IOllamaService _ollamaService;

        [ObservableProperty]
        public OllamaStatus _ollamaStatus;

        //public IRelayCommand NewEntryCommand { get; }

        public OllamaViewModel(IOllamaService ollamaService)
        : base(name: "Ollama", glyph: "\uE964", kind: AIServices.Ollama)
        {
            _ollamaService = ollamaService;
            _ollamaStatus = OllamaStatus.Unknown;

            _models = new ObservableCollection<ModelInfoViewModel>();

            _ = RefreshStatus();
        }

        private async Task RefreshStatus()
        {
            OllamaStatus = await _ollamaService.GetStatus();
        }

        private async Task RefreshModels()
        {
            if (OllamaStatus == OllamaStatus.Online)
            {
                var models = await _ollamaService.GetModels();

                foreach (var model in models) Models.Add(new ModelInfoViewModel(model));
            }
            else
            {
                Models.Clear();
            }
        }
    }
}