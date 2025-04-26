using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;
using PowerPad.Core.Services.AI;
using PowerPad.Core.Services.Config;
using System;
using System.Linq;
using System.Threading.Tasks;
using static PowerPad.WinUI.Configuration.ConfigConstants;

namespace PowerPad.WinUI.ViewModels.Settings
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly IConfigStore _configStore;

        public GeneralSettingsViewModel General { get; private init; }

        public ModelsSettingsViewModel Models { get; private init; }

        [ObservableProperty]
        public partial bool? IsAIAvailable { get; set; }

        public SettingsViewModel()
        {
            _configStore = App.Get<IConfigStore>();

            General = _configStore.Get<GeneralSettingsViewModel>(StoreKey.GeneralSettings);
            Models = _configStore.Get<ModelsSettingsViewModel>(StoreKey.ModelsSettings);

            General.PropertyChanged += (s, e) => _configStore.Set(StoreKey.GeneralSettings, General);
            Models.PropertyChanged += (s, e) => _configStore.Set(StoreKey.ModelsSettings, Models);

            General.ProviderAvaibilityChanged += UpdateAIAvaibility;
            Models.ModelAvaibilityChanged += UpdateAIAvaibility;

            General.InitializeAIServices();
        }

        public async Task TestConnections()
        {
            General.ProviderAvaibilityChanged -= UpdateAIAvaibility;

            try
            {
                if (General.OllamaEnabled)
                {
                    await General.OllamaConfig.TestConnection(App.Get<IAIService>(ModelProvider.Ollama));

                    if (General.OllamaAutostart && General.OllamaConfig.ServiceStatus == ServiceStatus.Available)
                    {
                        await App.Get<IOllamaService>().Start();
                        await General.OllamaConfig.TestConnection(App.Get<IAIService>(ModelProvider.Ollama));
                    }
                }
                else General.OllamaConfig.ResetStatus();

                if (General.AzureAIEnabled) await General.AzureAIConfig.TestConnection(App.Get<IAIService>(ModelProvider.GitHub));
                else General.AzureAIConfig.ResetStatus();

                if (General.OpenAIEnabled) await General.OpenAIConfig.TestConnection(App.Get<IAIService>(ModelProvider.OpenAI));
                else General.OpenAIConfig.ResetStatus();
            }
            finally
            {
                UpdateAIAvaibility(null, null);

                General.ProviderAvaibilityChanged += UpdateAIAvaibility;
            }
        }

        private void UpdateAIAvaibility(object? _, EventArgs? __)
        {
            var availableModels = Models.AvailableModels
                .Where(m => General.AvailableProviders.Contains(m.ModelProvider) && m.Enabled);
                
            if (!availableModels.Any())
            {
                Models.DefaultModel = null;
                IsAIAvailable = false;
            }
            else
            {
                if (Models.DefaultModel is null || !availableModels.Contains(Models.DefaultModel))
                {
                    Models.DefaultModel = availableModels.OrderBy(m => m.ModelProvider).First();
                }
                
                IsAIAvailable = true;
            }
        }
    }
}