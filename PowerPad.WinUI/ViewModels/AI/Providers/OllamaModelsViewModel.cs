using CommunityToolkit.Mvvm.Input;
using PowerPad.Core.Models.AI;
using PowerPad.Core.Services.AI;
using PowerPad.WinUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PowerPad.WinUI.ViewModels.AI.Providers
{
    public partial class OllamaModelsViewModel : AIModelsViewModelBase
    {
        private readonly IOllamaService _ollamaService;

        public IAsyncRelayCommand RefreshModelsCommand { get; }

        public OllamaModelsViewModel()
            : this(ModelProvider.Ollama)
        {
            _ = RefreshModels();
        }

        protected OllamaModelsViewModel(ModelProvider modelProvider)
            : base(modelProvider)
        {
            _ollamaService = App.Get<IOllamaService>();

            RefreshModelsCommand = new AsyncRelayCommand(RefreshModels);
        }

        protected async Task RefreshModels()
        {
            if (_settings.General.OllamaConfig.ServiceStatus != ServiceStatus.Online)
            {
                await _settings.General.OllamaConfig.TestConnection(_aiService);
            }

            if (_settings.General.OllamaConfig.ServiceStatus == ServiceStatus.Online)
            {
                IEnumerable<AIModel> newAvailableModels;

                newAvailableModels = await _ollamaService.GetInstalledModels();

                var currentAvailableModels = _settings.Models.AvailableModels;

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
        }

        protected override async Task SearchModels(string? query)
        {
            Searching = true;

            var resultModels = await _aiService.SearchModels(_modelProvider, query);

            SearchResultModels.Clear();
            SearchResultModels.AddRange(resultModels.Select(m =>
            {
                var existingModel = _settings.Models.AvailableModels
                    .FirstOrDefault(am => am.Name == m.Name && am.ModelProvider == m.ModelProvider);

                return existingModel ?? new(m)
                {
                    Available = false,
                    IsSizeTooLargeForExecution = m.Size.HasValue && m.Size > GC.GetGCMemoryInfo().TotalAvailableMemoryBytes
                };
            }));

            Searching = false;
        }

        protected override async Task AddModel(AIModelViewModel? aiModel)
        {
            ArgumentNullException.ThrowIfNull(aiModel);

            if (!_settings.Models.AvailableModels.Any(m => m == aiModel))
            {
                aiModel.Downloading = true;
                aiModel.DownloadCancelationToken = new CancellationTokenSource();
                aiModel.Available = false;

                _settings.Models.AvailableModels.Add(aiModel);

                await _ollamaService.DownloadModel
                (
                    aiModel.GetRecord(),
                    aiModel.UpdateDownloadProgress,
                    aiModel.UpdateDownloadError,
                    aiModel.DownloadCancelationToken.Token
                );
            }
        }

        protected override async Task RemoveModel(AIModelViewModel? aiModel)
        {
            ArgumentNullException.ThrowIfNull(aiModel);

            if (aiModel.Downloading)
            {
                await aiModel.DownloadCancelationToken!.CancelAsync();
            }
            else
            {
                await _ollamaService.DeleteModel(aiModel.GetRecord());
            }

            _settings.Models.AvailableModels.Remove(aiModel);
        }
    }
}