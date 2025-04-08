using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;
using PowerPad.Core.Services;
using PowerPad.Core.Services.AI;
using System.ComponentModel;
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
        private OllamaStatus _ollamaStatus;

        public SettingsViewModel()
        {
            _configStore = App.Get<IConfigStore>();

            General = _configStore.Get<GeneralSettingsViewModel>(StoreKey.GeneralSettings);
            Models = _configStore.Get<ModelsSettingsViewModel>(StoreKey.ModelsSettings);
            
            _ = UpdateOllamaStatus();

            General.PropertyChanged += SaveGeneralSettings;
            Models.PropertyChanged += SaveModelsSettings;
        }

        public static async Task<TestConnectionResult> TestConnection<TService>()
            where TService : IAIService
        {
            var service = App.Get<TService>();

            return await service.TestConection();
        }

        public async Task UpdateOllamaStatus()
        {
            OllamaStatus = await App.Get<IOllamaService>().GetStatus();
        }

        private void SaveGeneralSettings(object? _, PropertyChangedEventArgs eventArgs)
        {
            //Control for services config changes
            if (eventArgs.PropertyName == nameof(General.OllamaConfig))
            {
                App.Get<IOllamaService>().Initialize(General.OllamaConfig.GetRecord());
                _ = UpdateOllamaStatus();
            }
            else if (eventArgs.PropertyName == nameof(General.AzureAIConfig))
            {
                App.Get<IAzureAIService>().Initialize(General.AzureAIConfig.GetRecord());
            }
            else if (eventArgs.PropertyName == nameof(General.OpenAIConfig))
            {
                App.Get<IOpenAIService>().Initialize(General.OpenAIConfig.GetRecord());
            }

            _configStore.Set(StoreKey.GeneralSettings, General);
        }

        private void SaveModelsSettings(object? _, PropertyChangedEventArgs eventArgs)
        {
            //Control for change the default model
            if (eventArgs.PropertyName == nameof(Models.DefaultModel))
            {
                if (Models.DefaultModel is null)
                {
                    var availableModelProviders = General.GetAvailableModelProviders();

                    foreach (var modelProvider in availableModelProviders)
                    {
                        Models.DefaultModel = Models.AvailableModels.Where(m => m.ModelProvider == modelProvider).FirstOrDefault();

                        if (Models.DefaultModel is not null) break;
                    }
                }

                App.Get<IChatService>().SetDefaultModel(Models.DefaultModel?.GetRecord());
            }
            //Control for change the default AI parameters
            else if (eventArgs.PropertyName == nameof(Models.SendDefaultParameters)
                 || eventArgs.PropertyName == nameof(Models.DefaultParameters))
            {
                if (Models.SendDefaultParameters)
                    App.Get<IChatService>().SetDefaultParameters(Models.DefaultParameters.GetRecord());
                else
                    App.Get<IChatService>().SetDefaultParameters(null);
            }

            _configStore.Set(StoreKey.ModelsSettings, Models);
        }
    }
}
