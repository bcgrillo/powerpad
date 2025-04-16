using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;
using PowerPad.Core.Services.AI;
using PowerPad.Core.Services.Config;
using PowerPad.WinUI.Helpers;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using static PowerPad.WinUI.Configuration.ConfigConstants;

namespace PowerPad.WinUI.ViewModels.Settings
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly IConfigStore _configStore;
        private bool _modelMaintenanceInProgress;

        public GeneralSettingsViewModel General { get; private init; }

        public ModelsSettingsViewModel Models { get; private init; }

        [ObservableProperty]
        private OllamaStatus _ollamaStatus;

        public SettingsViewModel()
        {
            _configStore = App.Get<IConfigStore>();

            General = _configStore.Get<GeneralSettingsViewModel>(StoreKey.GeneralSettings);
            Models = _configStore.Get<ModelsSettingsViewModel>(StoreKey.ModelsSettings);

            if (General.OllamaEnabled)
            {
                _ = Task.Run(async () =>
                {
                    await UpdateOllamaStatus();

                    if (General.OllamaAutostart && OllamaStatus == OllamaStatus.Available)
                        await App.Get<IOllamaService>().Start();
                });
            }

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

            if (OllamaStatus == OllamaStatus.Online)
            {
                if (General.OllamaConfig.HasError)
                General.OllamaConfig.HasError = false;
            }
            else
            {
                General.OllamaConfig.HasError = true;
            }
        }

        public async Task<bool> IsAIAvailable()
        {
            if (General.OllamaEnabled) await UpdateOllamaStatus();

            if (General.AzureAIEnabled)
            {
                var test = await App.Get<IAzureAIService>().TestConection();
                General.AzureAIConfig.HasError = !test.Success;
            }
            if (General.OpenAIEnabled)
            {
                var test = await App.Get<IOpenAIService>().TestConection();
                General.OpenAIConfig.HasError = !test.Success;
            }

            EnsureDefaultModelFromAvailableProviders();

            if (General.OllamaEnabled
                && OllamaStatus == OllamaStatus.Online
                && Models.AvailableModels.Any(m => m.Enabled && m.ModelProvider == ModelProvider.Ollama))
            {
                return true;
            }
            else if (General.AzureAIEnabled
                && !General.AzureAIConfig.HasError
                && Models.AvailableModels.Any(m => m.Enabled && m.ModelProvider == ModelProvider.GitHub))
            {
                return true;
            }
            else if (General.OpenAIEnabled
                && !General.OpenAIConfig.HasError
                && Models.AvailableModels.Any(m => m.Enabled && m.ModelProvider == ModelProvider.OpenAI))
            {
                return true;
            }

            return false;
        }

        private void SaveGeneralSettings(object? _, PropertyChangedEventArgs eventArgs)
        {
            // Control for services config changes  
            if (General.OllamaEnabled && eventArgs.PropertyName == nameof(General.OllamaConfig))
            {
                OllamaStatus = OllamaStatus.Updating;
                App.Get<IOllamaService>().Initialize(General.OllamaConfig.GetRecord());

                _ = Task.Run(async () =>
                {
                    await UpdateOllamaStatus();
                });
            }
            else if (General.AzureAIEnabled && eventArgs.PropertyName == nameof(General.AzureAIConfig))
            {
                App.Get<IAzureAIService>().Initialize(General.AzureAIConfig.GetRecord());
                EnsureDefaultModelFromAvailableProviders();
            }
            else if (General.OpenAIEnabled && eventArgs.PropertyName == nameof(General.OpenAIConfig))
            {
                App.Get<IOpenAIService>().Initialize(General.OpenAIConfig.GetRecord());
                EnsureDefaultModelFromAvailableProviders();
            }

            else if (eventArgs.PropertyName == nameof(General.OllamaEnabled)
             || eventArgs.PropertyName == nameof(General.AzureAIEnabled)
             || eventArgs.PropertyName == nameof(General.OpenAIEnabled))
            {
                EnsureDefaultModelFromAvailableProviders();
            }
            else if (eventArgs.PropertyName == nameof(General.AcrylicBackground))
            {
                WindowHelper.MainWindow.SetBackdrop();
            }

            _configStore.Set(StoreKey.GeneralSettings, General);
        }

        private void SaveModelsSettings(object? _, PropertyChangedEventArgs eventArgs)
        {
            //Control for change the default model
            if (eventArgs.PropertyName == nameof(Models.DefaultModel))
            {
                if (Models.DefaultModel is null && Models.AvailableModels.Any(m => m.Enabled))
                    Models.DefaultModel = Models.AvailableModels.First(m => m.Enabled);

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
            else if (eventArgs.PropertyName == nameof(Models.AvailableModels))
            {
                EnsureDefaultModelFromAvailableProviders();
            }

            _configStore.Set(StoreKey.ModelsSettings, Models);
        }

        private void EnsureDefaultModelFromAvailableProviders()
        {
            // Avoid multiple calls to this method during the process
            if (_modelMaintenanceInProgress) return;

            try
            {
                _modelMaintenanceInProgress = true;
                
                var availableProviders = General.GetAvailableModelProviders();

                var disabledModels = Models.AvailableModels
                    .Where(model => !availableProviders.Contains(model.ModelProvider))
                    .ToList();

                foreach (var model in disabledModels)
                {
                    Models.AvailableModels.Remove(model);
                    Models.RecoverableModels.Add(model);
                }

                var recoverableModelsToEnable = Models.RecoverableModels
                    .Where(model => availableProviders.Contains(model.ModelProvider))
                    .ToList();

                foreach (var model in recoverableModelsToEnable)
                {
                    Models.RecoverableModels.Remove(model);
                    Models.AvailableModels.Add(model);
                }

                if (Models.AvailableModels.Any(m => m.Enabled))
                {
                    if (Models.DefaultModel is null || !Models.AvailableModels.Where(m => m.Enabled).Contains(Models.DefaultModel))
                    {
                        Models.DefaultModel = Models.AvailableModels.First(m => m.Enabled);
                    }
                }
                else
                {
                    if (Models.DefaultModel is not null) Models.DefaultModel = null;
                }
            }
            finally
            {
                _modelMaintenanceInProgress = false;
            }
        }
    }
}
