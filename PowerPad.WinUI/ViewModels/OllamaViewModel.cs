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

        public IRelayCommand RefreshStatusCommand { get; }
        public IRelayCommand RefreshModelsCommand { get; }

        public IEnumerable<ModelInfoViewModel>? OllamaModels => Models?.Where(model => model.ModelProvider == ModelProvider.Ollama);
        public IEnumerable<ModelInfoViewModel>? HuggingFaceModels => Models?.Where(model => model.ModelProvider == ModelProvider.HuggingFace);

        public OllamaViewModel(IOllamaService ollamaService)
        : base(name: "Ollama", provider: ModelProvider.Ollama)
        {
            _ollamaService = ollamaService;
            _ollamaStatus = OllamaStatus.Unknown;

            _ = RefreshStatus();
            _ = RefreshModels();

            RefreshStatusCommand = new RelayCommand(async () => await RefreshStatus());
            RefreshModelsCommand = new RelayCommand(async () => await RefreshModels());
        }

        private async Task RefreshStatus()
        {
            OllamaStatus = await _ollamaService.GetStatus();
        }

        private async Task RefreshModels()
        {
            await RefreshStatus();

            if (OllamaStatus == OllamaStatus.Online)
            {
                var models = await _ollamaService.GetModels();

                Models = [];

                foreach (var model in models)
                {
                    var displayName = model.Name.Replace("hf.co/", string.Empty).Replace("huggingface.co/", string.Empty);

                    Models.Add(new ModelInfoViewModel(model, displayName));
                }
            }
            else
            {
                Models = [];
            }

            OnPropertyChanged(nameof(OllamaModels));
            OnPropertyChanged(nameof(HuggingFaceModels));
        }
    }
}