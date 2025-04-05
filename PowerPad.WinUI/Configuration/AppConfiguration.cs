using Microsoft.Extensions.DependencyInjection;
using PowerPad.Core.Services;
using PowerPad.Core.Services.AI;
using PowerPad.Core.Services.Config;
using PowerPad.Core.Services.FileSystem;
using PowerPad.WinUI.ViewModels.Settings;
using System;
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
                var lastWorkspace = GetLastWorkspace(app);
                var configStoreService = sp.GetRequiredService<IConfigStoreService>();
                var orderService = sp.GetRequiredService<IOrderService>();

                return new(lastWorkspace, configStoreService, orderService);
            });
        }

        public static IServiceCollection ConfigureOllamaService(this IServiceCollection serviceColection, App app)
        {
            return serviceColection.AddSingleton<IOllamaService, OllamaService>(sp =>
            {
                var config = GetGeneralSettings(app.AppConfigStore).OllamaConfig;

                var ollama = new OllamaService();

                if (config is not null) ollama.Initialize(config.GetRecord());

                return ollama;
            });
        }

        public static IServiceCollection ConfigureAzureAIService(this IServiceCollection serviceColection, App app)
        {
            return serviceColection.AddSingleton<IAzureAIService, AzureAIService>(sp =>
            {
                var config = GetGeneralSettings(app.AppConfigStore).AzureAIConfig;

                var azureAI = new AzureAIService();

                if (config is not null) azureAI.Initialize(config.GetRecord());

                return azureAI;
            });
        }

        public static IServiceCollection ConfigureOpenAIService(this IServiceCollection serviceColection, App app)
        {
            return serviceColection.AddSingleton<IOpenAIService, OpenAIService>(sp =>
            {
                var config = GetGeneralSettings(app.AppConfigStore).OpenAIConfig;

                var openAI = new OpenAIService();

                if (config is not null) openAI.Initialize(config.GetRecord());

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

                var modelSettings = GetModelSettings(app.AppConfigStore);

                if (modelSettings.DefaultModel is not null) aiService.SetDefaultModel(modelSettings.DefaultModel.GetRecord());
                if (modelSettings.DefaultParameters is not null) aiService.SetDefaultParameters(modelSettings.DefaultParameters.GetRecord());

                return aiService;
            });
        }

        public static IConfigStore InitializeAppConfigStore(this App app)
        {
            var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $".{nameof(PowerPad).ToLower()}");
            var configStoreService = app.ServiceProvider.GetRequiredService<IConfigStoreService>();

            return configStoreService.GetConfigStore(appDataFolder);
        }

        private static string GetLastWorkspace(App app)
        {
            var recentlyWorkspaces = app.AppConfigStore.TryGet<string[]>(StoreKey.RecentlyWorkspaces);

            if (recentlyWorkspaces is null)
            {
                recentlyWorkspaces = [ StoreDefault.WorkspaceFolder ];
                app.AppConfigStore.Set(StoreKey.RecentlyWorkspaces, recentlyWorkspaces);
            }

            return recentlyWorkspaces.First();
        }

        private static GeneralSettingsViewModel GetGeneralSettings(IConfigStore config)
        {
            var general = config.TryGet<GeneralSettingsViewModel>(StoreKey.GeneralSettings);

            if (general is null)
            {
                general = StoreDefault.GeneralSettings;
                config.Set(StoreKey.GeneralSettings, general);
            }

            return general;
        }

        private static ModelsSettingsViewModel GetModelSettings(IConfigStore config)
        {
            var models = config.TryGet<ModelsSettingsViewModel>(StoreKey.ModelsSettings);

            if (models is null)
            {
                models = StoreDefault.GenerateDefaultModelsSettings();
                config.Set(StoreKey.ModelsSettings, models);
            }

            return models;
        }
    }
}