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
using static PowerPad.WinUI.Configuration.ConfigConstants;

namespace PowerPad.WinUI.Configuration
{
    /// <summary>
    /// Provides configuration methods for setting up application services and initializing the application configuration store.
    /// </summary>
    public static class AppConfiguration
    {
        /// <summary>
        /// Configures the workspace service.
        /// </summary>
        /// <param name="serviceCollection">The service collection to which the workspace service will be added.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection ConfigureWorkspaceService(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton<IWorkspaceService, WorkspaceService>(sp =>
            {
                var lastWorkspace = sp.GetRequiredService<IConfigStore>().Get<ObservableCollection<string>>(StoreKey.RecentlyWorkspaces)[0];
                var orderService = sp.GetRequiredService<IOrderService>();

                return new(lastWorkspace, orderService);
            });
        }

        /// <summary>
        /// Configures the Ollama AI service.
        /// </summary>
        /// <param name="serviceCollection">The service collection to which the Ollama service will be added.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection ConfigureOllamaService(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton<OllamaService>()
                .AddSingleton<IOllamaService, OllamaService>(sp => sp.GetRequiredService<OllamaService>())
                .AddKeyedSingleton<IAIService, OllamaService>(ModelProvider.Ollama, (sp, _) => sp.GetRequiredService<OllamaService>())
                .AddKeyedSingleton<IAIService, OllamaService>(ModelProvider.HuggingFace, (sp, _) => sp.GetRequiredService<OllamaService>());
        }

        /// <summary>
        /// Configures the Azure AI service.
        /// </summary>
        /// <param name="serviceCollection">The service collection to which the Azure AI service will be added.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection ConfigureAzureAIService(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddKeyedSingleton<IAIService, AzureAIService>(ModelProvider.GitHub);
        }

        /// <summary>
        /// Configures the OpenAI service.
        /// </summary>
        /// <param name="serviceCollection">The service collection to which the OpenAI service will be added.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection ConfigureOpenAIService(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddKeyedSingleton<IAIService, OpenAIService>(ModelProvider.OpenAI);
        }

        /// <summary>
        /// Configures the AI service, including chat services for multiple AI providers.
        /// </summary>
        /// <param name="serviceCollection">The service collection to which the AI service will be added.</param>
        /// <returns>The updated service collection.</returns>
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

        /// <summary>
        /// Configures the configuration store service.
        /// </summary>
        /// <param name="serviceCollection">The service collection to which the configuration store service will be added.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection ConfigureConfigStoreService(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton<IConfigStoreService, ConfigStoreService>(sp => new(AppJsonContext.Custom));
        }

        /// <summary>
        /// Configures the order service.
        /// </summary>
        /// <param name="serviceCollection">The service collection to which the order service will be added.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection ConfigureOrderService(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton<IOrderService, OrderService>(sp => new(AppJsonContext.Custom));
        }

        /// <summary>
        /// Initializes the application configuration store and sets default values if necessary.
        /// </summary>
        /// <param name="appConfigStore">The output parameter that will hold the initialized configuration store.</param>
        public static void InitializeAppConfigStore(out IConfigStore appConfigStore)
        {
            var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $".{nameof(PowerPad).ToLower()}");
            var configStoreService = App.Get<IConfigStoreService>();

            appConfigStore = configStoreService.GetConfigStore(appDataFolder);

            // Initialize recently used workspaces if necessary
            var recentlyWorkspaces = appConfigStore.TryGet<ObservableCollection<string>>(StoreKey.RecentlyWorkspaces);

            if (recentlyWorkspaces is null)
            {
                recentlyWorkspaces = [StoreDefault.WorkspaceFolder];
                appConfigStore.Set(StoreKey.RecentlyWorkspaces, recentlyWorkspaces);
            }

            // Initialize general settings if necessary
            var generalSettings = appConfigStore.TryGet<GeneralSettingsViewModel>(StoreKey.GeneralSettings);

            if (generalSettings is null)
            {
                generalSettings = StoreDefault.GeneralSettings;
                appConfigStore.Set(StoreKey.GeneralSettings, generalSettings);
            }

            // Initialize model settings if necessary
            var modelsSettings = appConfigStore.TryGet<ModelsSettingsViewModel>(StoreKey.ModelsSettings);

            if (modelsSettings is null)
            {
                modelsSettings = StoreDefault.GenerateDefaultModelsSettings();
                appConfigStore.Set(StoreKey.ModelsSettings, modelsSettings);
            }

            // Initialize agents if necessary
            var agents = appConfigStore.TryGet<ObservableCollection<AgentViewModel>>(StoreKey.Agents);

            if (agents is null)
            {
                agents = StoreDefault.AgentsCollection;
                appConfigStore.Set(StoreKey.Agents, agents);
            }
        }
    }
}