using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using PowerPad.Core.Models.Config;
using PowerPad.Core.Services;
using PowerPad.Core.Services.AI;
using PowerPad.WinUI.Messages;
using System.Threading.Tasks;
using static PowerPad.WinUI.Configuration.ConfigConstants;

namespace PowerPad.WinUI.ViewModels.Settings
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly IConfigStore _configStore;
        private readonly IOllamaService _ollama;
        private readonly IAzureAIService _azureAI;
        private readonly IOpenAIService _openAI;

        public GeneralSettingsViewModel General { get; private init; }

        public ModelsSettingsViewModel Models { get; private init; }

        [ObservableProperty]
        private OllamaStatus _ollamaStatus;

        public SettingsViewModel()
        {
            _configStore = App.Get<IConfigStore>();
            _ollama = App.Get<IOllamaService>();
            _azureAI = App.Get<IAzureAIService>();
            _openAI = App.Get<IOpenAIService>();

            General = _configStore.Get<GeneralSettingsViewModel>(StoreKey.GeneralSettings);
            Models = _configStore.Get<ModelsSettingsViewModel>(StoreKey.ModelsSettings);

            _ = Task.Run(async() =>
            {
                OllamaStatus = await _ollama.GetStatus();
            });

            General.PropertyChanged += (s, e) => SaveGeneralSettings();
            Models.PropertyChanged += (s, e) => SaveModelsSettings();
        }

        private void SaveGeneralSettings() => _configStore.Set(StoreKey.GeneralSettings, General);

        private void SaveModelsSettings() => _configStore.Set(StoreKey.ModelsSettings, Models);
    }
}
