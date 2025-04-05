using CommunityToolkit.Mvvm.Input;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;
using PowerPad.Core.Models.Config;
using PowerPad.Core.Services.AI;
using PowerPad.WinUI.ViewModels.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerPad.WinUI.ViewModels.AI
{
    public partial class OllamaModelsViewModel : AIModelsViewModelBase
    {
        public OllamaStatus OllamaStatus { get; private set; }

        public RelayCommand RefreshModelsCommand { get; }

        public OllamaModelsViewModel()
            : this(App.Get<IOllamaService>(), ModelProvider.Ollama)
        {
        }

        protected OllamaModelsViewModel(IAIService aiService, ModelProvider modelProvider)
            : base(aiService, modelProvider)
        {
            RefreshModelsCommand = new(async () => await RefreshModels());

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
                    if (!currentAvailableModels.Any(m => m.GetRecord() == newAvailableModel))
                    {
                        currentAvailableModels.Add(new(newAvailableModel));
                    }
                }

                for (int i = currentAvailableModels.Count - 1; i >= 0; i--)
                {
                    var currentAvailableModel = currentAvailableModels[i];

                    if (currentAvailableModel.ModelProvider is ModelProvider.Ollama or ModelProvider.HuggingFace)
                    {
                        var newAvailableModel = newAvailableModels.FirstOrDefault(m => m == currentAvailableModel.GetRecord());

                        if (newAvailableModel is null)
                        {
                            if (!currentAvailableModel.Downloading) currentAvailableModels.RemoveAt(i);
                        }
                        else
                        {
                            currentAvailableModel.Available = true;
                            currentAvailableModel.Downloading = false;
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

            FilterModels(null, null);
        }
    }
}