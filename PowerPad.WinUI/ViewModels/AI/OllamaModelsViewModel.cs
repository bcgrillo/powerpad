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
        public OllamaModelsViewModel()
            : base(App.Get<IOllamaService>(), ModelProvider.Ollama)
        {
            _ = RefreshModels();
        }

        private async Task RefreshModels()
        {
            var ollamaService = (IOllamaService)_aiService;

            var ollamaStatus = await ollamaService.GetStatus();

            IEnumerable<AIModel> newAvailableModels;

            if (ollamaStatus == OllamaStatus.Online)
            {
                newAvailableModels = await ollamaService.GetAvailableModels();
            }
            else
            {
                newAvailableModels = [];
            }

            var currentAvailableModels = _settingsViewModel.Models.AvailableModels;

            foreach (var model in newAvailableModels)
            {
                if (!currentAvailableModels.Any(m => m.GetModel() == model))
                {
                    currentAvailableModels.Add(new(model));
                }
            }

            for (int i = currentAvailableModels.Count - 1; i >= 0; i--)
            {
                var model = currentAvailableModels[i];
                if (!newAvailableModels.Any(m => m == model.GetModel()) &&
                    (model.ModelProvider == ModelProvider.Ollama || model.ModelProvider == ModelProvider.HuggingFace))
                {
                    currentAvailableModels.RemoveAt(i);
                }
            }

            FilterModels(null, null);
        }
    }
}