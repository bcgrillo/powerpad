using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using PowerPad.Core.Models;
using PowerPad.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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

                return new WorkspaceService(lastWorkspace, configStoreService, orderService);
            });
        }

        public static IServiceCollection ConfigureOllamaService(this IServiceCollection serviceColection, App app)
        {
            return serviceColection.AddSingleton<IOllamaService, OllamaService>(sp =>
            {
                var config = GetGeneralSettings(app.AppConfigStore).OllamaConfig;

                var ollama = new OllamaService();

                if (config != null) ollama.Initialize(config);

                return ollama;
            });
        }

        public static IServiceCollection ConfigureAzureAIService(this IServiceCollection serviceColection, App app)
        {
            return serviceColection.AddSingleton<IAzureAIService, AzureAIService>(sp =>
            {
                var config = GetGeneralSettings(app.AppConfigStore).AzureAIConfig;

                var azureAI = new AzureAIService();

                if (config != null) azureAI.Initialize(config);

                return azureAI;
            });
        }

        public static IServiceCollection ConfigureOpenAIService(this IServiceCollection serviceColection, App app)
        {
            return serviceColection.AddSingleton<IOpenAIService, OpenAIService>(sp =>
            {
                var config = GetGeneralSettings(app.AppConfigStore).OpenAIConfig;

                var openAI = new OpenAIService();

                if (config != null) openAI.Initialize(config);

                return openAI;
            });
        }

        public static IServiceCollection ConfigureAIService(this IServiceCollection serviceColection, App app)
        {
            return serviceColection.AddSingleton<IAIService, AIService>(sp =>
            {
                var ollamaService = sp.GetRequiredService<IOllamaService>();
                var azureAIService = sp.GetRequiredService<IAzureAIService>();
                var openAIService = sp.GetRequiredService<IOpenAIService>();

                var aiService = new AIService(ollamaService, azureAIService, openAIService);

                var modelSettings = GetModelSettings(app.AppConfigStore);

                if (modelSettings.DefaultModel != null) aiService.SetDefaultModel(modelSettings.DefaultModel);
                if (modelSettings.DefaultParameters != null) aiService.SetDefaultParameters(modelSettings.DefaultParameters);

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

            if (recentlyWorkspaces == null)
            {
                recentlyWorkspaces = [ StoreDefault.WorkspaceFolder ];
                app.AppConfigStore.Set(StoreKey.RecentlyWorkspaces, recentlyWorkspaces);
            }

            return recentlyWorkspaces.First();
        }

        private static GeneralSettings GetGeneralSettings(IConfigStore config)
        {
            var general = config.TryGet<GeneralSettings>(StoreKey.GeneralSettings);

            if (general == null)
            {
                general = StoreDefault.GeneralSettings;
                config.Set(StoreKey.GeneralSettings, general);
            }

            return general;
        }

        private static ModelsSettings GetModelSettings(IConfigStore config)
        {
            var models = config.TryGet<ModelsSettings>(StoreKey.ModelsSettings);

            if (models == null)
            {
                models = StoreDefault.ModelsSettings;
                config.Set(StoreKey.ModelsSettings, models);
            }

            return models;
        }
    }
}