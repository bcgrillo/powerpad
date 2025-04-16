using Microsoft.Extensions.DependencyInjection;
using PowerPad.Core.Services.AI;
using PowerPad.Core.Services.Config;
using PowerPad.Core.Services.FileSystem;
using PowerPad.WinUI.ViewModels.Agents;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using static PowerPad.WinUI.Configuration.ConfigConstants;

namespace PowerPad.WinUI.Configuration
{
    public static class AppConfiguration
    {
        public static IServiceCollection ConfigureWorkspaceService(this IServiceCollection serviceColection, App app)
        {
            return serviceColection.AddSingleton<IWorkspaceService, WorkspaceService>(sp =>
            {
                var lastWorkspace = app.AppConfigStore.Get<string[]>(StoreKey.RecentlyWorkspaces).First();
                var configStoreService = sp.GetRequiredService<IConfigStoreService>();
                var orderService = sp.GetRequiredService<IOrderService>();

                return new(lastWorkspace, configStoreService, orderService);
            });
        }

        public static IServiceCollection ConfigureOllamaService(this IServiceCollection serviceColection, App app)
        {
            return serviceColection.AddSingleton<IOllamaService, OllamaService>(sp =>
            {
                var generalSettings = app.AppConfigStore.Get<GeneralSettingsViewModel>(StoreKey.GeneralSettings);

                var ollama = new OllamaService();

                if (generalSettings.OllamaEnabled) ollama.Initialize(generalSettings.OllamaConfig.GetRecord());

                return ollama;
            });
        }

        public static IServiceCollection ConfigureAzureAIService(this IServiceCollection serviceColection, App app)
        {
            return serviceColection.AddSingleton<IAzureAIService, AzureAIService>(sp =>
            {
                var generalSettings = app.AppConfigStore.Get<GeneralSettingsViewModel>(StoreKey.GeneralSettings);

                var azureAI = new AzureAIService();

                if (generalSettings.AzureAIEnabled) azureAI.Initialize(generalSettings.AzureAIConfig.GetRecord());

                return azureAI;
            });
        }

        public static IServiceCollection ConfigureOpenAIService(this IServiceCollection serviceColection, App app)
        {
            return serviceColection.AddSingleton<IOpenAIService, OpenAIService>(sp =>
            {
                var generalSettings = app.AppConfigStore.Get<GeneralSettingsViewModel>(StoreKey.GeneralSettings);

                var openAI = new OpenAIService();

                if (generalSettings.OpenAIEnabled) openAI.Initialize(generalSettings.OpenAIConfig.GetRecord());

                return openAI;
            });
        }

        public static IServiceCollection ConfigureAIService(this IServiceCollection serviceColection, App app)
        {
            return serviceColection.AddSingleton<IChatService, ChatService>(sp =>
            {
                var ollamaService = sp.GetRequiredService<IOllamaService>();
                var azureAIService = sp.GetRequiredService<IAzureAIService>();
                var openAIService = sp.GetRequiredService<IOpenAIService>();

                var aiService = new ChatService(ollamaService, azureAIService, openAIService);

                var modelSettings = app.AppConfigStore.Get<ModelsSettingsViewModel>(StoreKey.ModelsSettings);

                if (modelSettings.DefaultModel is not null) aiService.SetDefaultModel(modelSettings.DefaultModel.GetRecord());
                if (modelSettings.DefaultParameters is not null) aiService.SetDefaultParameters(modelSettings.DefaultParameters.GetRecord());

                return aiService;
            });
        }

        public static IConfigStore InitializeAppConfigStore(this App app)
        {
            var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $".{nameof(PowerPad).ToLower()}");
            var configStoreService = app.ServiceProvider.GetRequiredService<IConfigStoreService>();

            var appConfigStore = configStoreService.GetConfigStore(appDataFolder);

            //Initialize recently workspaces if necessary
            var recentlyWorkspaces = appConfigStore.TryGet<string[]>(StoreKey.RecentlyWorkspaces);

            if (recentlyWorkspaces is null)
            {
                recentlyWorkspaces = [StoreDefault.WorkspaceFolder];
                appConfigStore.Set(StoreKey.RecentlyWorkspaces, recentlyWorkspaces);
            }

            //Initialize general settings if necessary
            var generalSettings = appConfigStore.TryGet<GeneralSettingsViewModel>(StoreKey.GeneralSettings);

            if (generalSettings is null)
            {
                generalSettings = StoreDefault.GeneralSettings;
                appConfigStore.Set(StoreKey.GeneralSettings, generalSettings);
            }

            //Initizalize models settings if necessary
            var modelsSettings = appConfigStore.TryGet<ModelsSettingsViewModel>(StoreKey.ModelsSettings);

            if (modelsSettings is null)
            {
                modelsSettings = StoreDefault.GenerateDefaultModelsSettings();
                appConfigStore.Set(StoreKey.ModelsSettings, modelsSettings);
            }

            //Initizalize agents if necessary
            var agents = appConfigStore.TryGet<ObservableCollection<AgentViewModel>>(StoreKey.Agents);

            if (agents is null)
            {
                agents = StoreDefault.AgentsCollection;
                appConfigStore.Set(StoreKey.Agents, agents);
            }

            return appConfigStore;
        }
    }
}