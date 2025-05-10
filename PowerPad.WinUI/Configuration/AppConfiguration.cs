using Microsoft.Extensions.DependencyInjection;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;
using PowerPad.Core.Services.AI;
using PowerPad.Core.Services.Config;
using PowerPad.Core.Services.FileSystem;
using PowerPad.WinUI.ViewModels.Agents;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using static PowerPad.WinUI.Configuration.ConfigConstants;

namespace PowerPad.WinUI.Configuration
{
    public static class AppConfiguration
    {
        public static IServiceCollection ConfigureWorkspaceService(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton<IWorkspaceService, WorkspaceService>(sp =>
            {
                var lastWorkspace = sp.GetRequiredService<IConfigStore>().Get<ObservableCollection<string>>(StoreKey.RecentlyWorkspaces).First();
                var orderService = sp.GetRequiredService<IOrderService>();

                return new(lastWorkspace, orderService);
            });
        }

        public static IServiceCollection ConfigureOllamaService(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton<OllamaService>()
                .AddSingleton<IOllamaService, OllamaService>(sp => sp.GetRequiredService<OllamaService>())
                .AddKeyedSingleton<IAIService, OllamaService>(ModelProvider.Ollama, (sp, _) => sp.GetRequiredService<OllamaService>())
                .AddKeyedSingleton<IAIService, OllamaService>(ModelProvider.HuggingFace, (sp, _) => sp.GetRequiredService<OllamaService>());
        }

        public static IServiceCollection ConfigureAzureAIService(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddKeyedSingleton<IAIService, AzureAIService>(ModelProvider.GitHub);
        }

        public static IServiceCollection ConfigureOpenAIService(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddKeyedSingleton<IAIService, OpenAIService>(ModelProvider.OpenAI);
        }

        public static IServiceCollection ConfigureAIService(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton<IChatService, ChatService>(sp =>
            {
                var aiServices = new Dictionary<ModelProvider, IAIService>
                {
                    { ModelProvider.Ollama, sp.GetRequiredKeyedService<IAIService>(ModelProvider.Ollama) },
                    { ModelProvider.HuggingFace, sp.GetRequiredKeyedService<IAIService>(ModelProvider.HuggingFace) },
                    { ModelProvider.GitHub, sp.GetRequiredKeyedService<IAIService>(ModelProvider.GitHub) },
                    { ModelProvider.OpenAI, sp.GetRequiredKeyedService<IAIService>(ModelProvider.OpenAI) }
                };

                var aiService = new ChatService(aiServices.AsReadOnly());

                return aiService;
            });
        }

        public static IServiceCollection ConfigureConfigStoreService(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton<IConfigStoreService, ConfigStoreService>(sp => new(AppJsonContext.Custom));
        }

        public static IServiceCollection ConfigureOrderService(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton<IOrderService, OrderService>(sp => new(AppJsonContext.Custom));
        }

        public static void InitializeAppConfigStore(out IConfigStore appConfigStore)
        {
            var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $".{nameof(PowerPad).ToLower()}");
            var configStoreService = App.Get<IConfigStoreService>();

            appConfigStore = configStoreService.GetConfigStore(appDataFolder);

            //Initialize recently workspaces if necessary
            var recentlyWorkspaces = appConfigStore.TryGet<ObservableCollection<string>>(StoreKey.RecentlyWorkspaces);

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

            //Initialize models settings if necessary
            var modelsSettings = appConfigStore.TryGet<ModelsSettingsViewModel>(StoreKey.ModelsSettings);

            if (modelsSettings is null)
            {
                modelsSettings = StoreDefault.GenerateDefaultModelsSettings();
                appConfigStore.Set(StoreKey.ModelsSettings, modelsSettings);
            }

            //Initialize agents if necessary
            var agents = appConfigStore.TryGet<ObservableCollection<AgentViewModel>>(StoreKey.Agents);

            if (agents is null)
            {
                agents = StoreDefault.AgentsCollection;
                appConfigStore.Set(StoreKey.Agents, agents);
            }
        }
    }
}