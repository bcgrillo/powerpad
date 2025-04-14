using CommunityToolkit.Mvvm.Input;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;
using PowerPad.Core.Services.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerPad.WinUI.ViewModels.AI
{
    public partial class OllamaModelsViewModel : AIModelsViewModelBase
    {
        public OllamaStatus OllamaStatus { get; private set; }

        public IRelayCommand RefreshModelsCommand { get; }

        public OllamaModelsViewModel()
            : this(App.Get<IOllamaService>(), ModelProvider.Ollama)
        {
        }

        protected OllamaModelsViewModel(IAIService aiService, ModelProvider modelProvider)
            : base(aiService, modelProvider)
        {
            RefreshModelsCommand = new RelayCommand(async () => await RefreshModels());

            _ = RefreshModels();
        }

        private async Task RefreshModels()
        {
            var ollamaService = (IOllamaService)_aiService;

            OllamaStatus = await ollamaService.GetStatus();
            OnPropertyChanged(nameof(OllamaStatus));

            IEnumerable<AIModel> newAvailableModels;

            if (OllamaStatus == OllamaStatus.Online)
            {
                newAvailableModels = await ollamaService.GetAvailableModels();

                var currentAvailableModels = _settingsViewModel.Models.AvailableModels;

                foreach (var newAvailableModel in newAvailableModels)
                {
                    if (!currentAvailableModels.Any(m => m.Name == newAvailableModel.Name && m.ModelProvider == newAvailableModel.ModelProvider))
                    {
                        currentAvailableModels.Add(new(newAvailableModel));
                    }
                }

                for (int i = currentAvailableModels.Count - 1; i >= 0; i--)
                {
                    var currentAvailableModel = currentAvailableModels[i];

                    if (currentAvailableModel.ModelProvider is ModelProvider.Ollama or ModelProvider.HuggingFace)
                    {
                        var newAvailableModel = newAvailableModels
                            .FirstOrDefault(m => m.Name == currentAvailableModel.Name && m.ModelProvider == currentAvailableModel.ModelProvider);

                        if (newAvailableModel is null)
                        {
                            if (!currentAvailableModel.Downloading) currentAvailableModels.RemoveAt(i);
                        }
                    }
                }
            }
            else
            {
                foreach (var model in _settingsViewModel.Models.AvailableModels)
                {
                    if (model.ModelProvider == ModelProvider.Ollama || model.ModelProvider == ModelProvider.HuggingFace)
                    {
                        model.Available = false;
                    }
                }
            }
        }

        protected override async Task SearchModels(string? query)
        {
            Searching = true;

            var resultModels = await _aiService.SearchModels(_modelProvider, query);

            SearchResultModels.Clear();
            SearchResultModels.AddRange(resultModels.Select(m =>
            {
                var existingModel = _settingsViewModel.Models.AvailableModels
                    .FirstOrDefault(am => am.Name == m.Name && am.ModelProvider == m.ModelProvider);

                return existingModel ?? new(m) { Available = false };
            }));

            Searching = false;
        }

        protected override async Task AddModel(AIModelViewModel? aiModel)
        {
            ArgumentNullException.ThrowIfNull(aiModel);

            if (!_settingsViewModel.Models.AvailableModels.Any(m => m == aiModel))
            {
                aiModel.Downloading = true;
                aiModel.Available = false;

                _settingsViewModel.Models.AvailableModels.Add(aiModel);

                await ((IOllamaService)_aiService).Download
                (
                    aiModel.GetRecord(),
                    aiModel.UpdateDownloadProgess,
                    aiModel.UpdateDownloadError
                );
            }
        }

        protected override async Task RemoveModel(AIModelViewModel? aiModel)
        {
            ArgumentNullException.ThrowIfNull(aiModel);

            await ((IOllamaService)_aiService).RemoveModel(aiModel.GetRecord());

            _settingsViewModel.Models.AvailableModels.Remove(aiModel);
        }
    }
}