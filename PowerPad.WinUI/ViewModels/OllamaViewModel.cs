using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OllamaSharp.Models;
using PowerPad.Core.Models;
using PowerPad.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.WinUI.ViewModels
{
    public partial class OllamaViewModel : ObservableObject
    {
        private readonly IOllamaService _ollamaService;
        private readonly SettingsViewModel _settingsViewModel;

        [ObservableProperty]
        private OllamaStatus _ollamaStatus;

        public IRelayCommand RefreshStatusCommand { get; }
        public IRelayCommand RefreshModelsCommand { get; }
        public IRelayCommand<AIModel> SetDefaultModel { get; }


        public ObservableCollection<AIModelViewModel> OllamaModels { get; set; }

        public ObservableCollection<AIModelViewModel> HuggingFaceModels { get; set; }

        public OllamaViewModel()
        {
            _ollamaService = App.Get<IOllamaService>();
            _settingsViewModel = App.Get<SettingsViewModel>();

            _ollamaStatus = OllamaStatus.Unknown;

            OllamaModels = [];
            HuggingFaceModels = [];

            InitCollections();

            _ = RefreshStatus();

            RefreshStatusCommand = new RelayCommand(async () => await RefreshStatus());
            RefreshModelsCommand = new RelayCommand(async () => await RefreshModels());
            SetDefaultModel = new RelayCommand<AIModel>(model => _settingsViewModel.Models.DefaultModel = model);
        }

        private void InitCollections()
        {
            foreach (var model in _settingsViewModel.Models.AvailableModels)
            {
                AIModelViewModel? viewModel = null;

                if (model.ModelProvider == ModelProvider.Ollama)
                {
                    viewModel = new AIModelViewModel(model);
                    OllamaModels.Add(viewModel);
                }
                else if (model.ModelProvider == ModelProvider.HuggingFace)
                {
                    viewModel = new AIModelViewModel(model);
                    HuggingFaceModels.Add(viewModel);
                }
            }
        }

        private async Task RefreshStatus()
        {
            OllamaStatus = await _ollamaService.GetStatus();
        }

        private async Task RefreshModels()
        {
            await RefreshStatus();

            IEnumerable<AIModel> availableModels;

            if (OllamaStatus == OllamaStatus.Online)
            {
                availableModels = await _ollamaService.GetAvailableModels();
            }
            else
            {
                availableModels = Enumerable.Empty<AIModel>();
            }

            var destinationModels = _settingsViewModel.Models.AvailableModels;

            foreach (var model in availableModels)
            {
                if (!destinationModels.Any(m => m == model)) destinationModels.Add(model);
            }

            for (int i = destinationModels.Count - 1; i >= 0; i--)
            {
                var model = destinationModels[i];
                if (!availableModels.Any(m => m == model) &&
                    (model.ModelProvider == ModelProvider.GitHub || model.ModelProvider == ModelProvider.HuggingFace))
                {
                    destinationModels.RemoveAt(i);
                }
            }
        }
    }
}