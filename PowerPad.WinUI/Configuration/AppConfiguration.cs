using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using PowerPad.Core.Configuration;
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

                return new WorkspaceService(lastWorkspace, configStoreService);
            });
        }

        public static IServiceCollection ConfigureOllamaService(this IServiceCollection serviceColection, App app)
        {
            return serviceColection.AddSingleton<IOllamaService, OllamaService>(sp =>
            {
                string ollamaServiceUrl = GetOllamaServiceUrl(app.AppConfigStore);

                return new OllamaService(ollamaServiceUrl);
            });
        }

        public static IServiceCollection ConfigureAzureAIService(this IServiceCollection serviceColection, App app)
        {
            return serviceColection.AddSingleton<IAzureAIService, AzureAIService>(sp =>
            {
                var (baseUrl, key) = GetAzureAIConfig(app.AppConfigStore);

                return new AzureAIService(baseUrl, key, app.AppConfigStore);
            });
        }

        public static IServiceCollection ConfigureOpenAIService(this IServiceCollection serviceColection, App app)
        {
            return serviceColection.AddSingleton<IOpenAIService, OpenAIService>(sp =>
            {
                var (baseUrl, key) = GetOpenAIConfig(app.AppConfigStore);

                return new OpenAIService(baseUrl, key, app.AppConfigStore);
            });
        }

        public static IServiceCollection ConfigureAIService(this IServiceCollection serviceColection, App app)
        {
            return serviceColection.AddSingleton<IAIService, AIService>(sp =>
            {
                AIModel defaultModel = GetDefaultModel(app.AppConfigStore);
                var ollamaService = sp.GetRequiredService<IOllamaService>();
                var azureAIService = sp.GetRequiredService<IAzureAIService>();
                var openAIService = sp.GetRequiredService<IOpenAIService>();

                return new AIService(defaultModel, ollamaService, azureAIService, openAIService);
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
            var recentlyWorkspaces = app.AppConfigStore.TryGet<string[]>(Keys.RecentlyWorkspaces);

            if (recentlyWorkspaces == null)
            {
                recentlyWorkspaces = [ Defaults.WorkspaceFolder ];
                app.AppConfigStore.Set(Keys.RecentlyWorkspaces, recentlyWorkspaces);
            }

            return recentlyWorkspaces.First();
        }

        private static string GetOllamaServiceUrl(IConfigStore config)
        {
            var configUrl = config.TryGet<string>(Keys.OllamaServiceUrl);

            if (configUrl == null)
            {
                configUrl = Defaults.OllamaServiceUrl;
                config.Set(Keys.OllamaServiceUrl, configUrl);
            }

            return configUrl;
        }

        private static AzureAIConfig GetAzureAIConfig(IConfigStore config)
        {
            var azureAIConfig = config.TryGet<AzureAIConfig>(Keys.AzureAIConfig);

            if (azureAIConfig == null)
            {
                azureAIConfig = Defaults.AzureAIConfig;
                config.Set(Keys.AzureAIConfig, azureAIConfig);
            }

            return azureAIConfig;
        }

        private static OpenAIConfig GetOpenAIConfig(IConfigStore config)
        {
            var openAIConfig = config.TryGet<OpenAIConfig>(Keys.OpenAIConfig);

            if (openAIConfig == null)
            {
                openAIConfig = Defaults.OpenAIConfig;
                config.Set(Keys.AzureAIConfig, openAIConfig);
            }

            return openAIConfig;
        }

        private static AIModel GetDefaultModel(IConfigStore config)
        {
            var configModel = config.TryGet<AIModel>(Keys.DefaultModel);

            if (configModel == null)
            {
                configModel = Defaults.DefaultModel;
                config.Set(Keys.DefaultModel, configModel);
            }

            return configModel;
        }
    }
}