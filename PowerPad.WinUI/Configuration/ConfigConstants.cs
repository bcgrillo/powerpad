using PowerPad.Core.Models.AI;
using PowerPad.WinUI.ViewModels.AI;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PowerPad.WinUI.Configuration
{
    public static class ConfigConstants
    {
        public enum StoreKey
        {
            RecentlyWorkspaces,
            GeneralSettings,
            ModelsSettings,
            GitHubModels,
            OpenAIModels,
            CurrentDocumentPath,
        }

        public class StoreDefault
        {
            private static readonly string[] _initialGitHubModels =
                ["gpt-4o", "gpt-4o-mini", "DeepSeek-R1", "DeepSeek-V3", "Llama-3-3-70B-Instruct", "Phi-4"];
            private static readonly string[] _initialOpenAIModels =
                ["gpt-4o", "gpt-4o-mini", "o3-mini", "o1-mini", "o1"];

            public static readonly string WorkspaceFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), nameof(PowerPad));

            public static readonly GeneralSettingsViewModel GeneralSettings = new()
            {
                OllamaEnabled = true,
                AzureAIEnabled = false,
                OpenAIEnabled = false,

                OllamaConfig = new(new() { BaseUrl = "http://localhost:11434" }),
                //TODO: Remove
                AzureAIConfig = new(new() { BaseUrl = "https://models.inference.ai.azure.com", Key = "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" }),
                OpenAIConfig = new(new() { BaseUrl = "https://api.openai.com/v1", Key = "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" }),

                AppTheme = null, //Use system configuration
                AcrylicBackground = true,
            };

            public static ModelsSettingsViewModel GenerateDefaultModelsSettings()
            {
                var defaultModelSettings = new ModelsSettingsViewModel
                {
                    DefaultModel = new(new("gemma3:latest", ModelProvider.Ollama, 3338801718), true),
                    DefaultParameters = new(new()
                    {
                        SystemPrompt = "Eres PowerPad, un asistente de inteligencia artificial amable y resolutivo.",
                        Temperature = 0.7f,
                        TopP = 1,
                        MaxOutputTokens = 1000,
                        MaxConversationLength = 50
                    }),
                    SendDefaultParameters = true,
                    AvailableModels = []
                };

                defaultModelSettings.AvailableModels.Add(defaultModelSettings.DefaultModel);
                defaultModelSettings.AvailableModels.AddRange(_initialGitHubModels.Select(m => new AIModelViewModel(new(m, ModelProvider.GitHub), true)));
                defaultModelSettings.AvailableModels.AddRange(_initialOpenAIModels.Select(m => new AIModelViewModel(new(m, ModelProvider.OpenAI), true)));

                return defaultModelSettings;
            }

            //public static AgentViewModel DefaultAgent = new("PowerPad", "Eres un editor de texto que cumples la acción solicitada por el usuario")
            //{
            //    Description = ,
            //    PromptParameterName = "Acción",
            //    PromptParameterPlaceholder = "¿Qué quieres hacer?",

            //    Parameters = new(new()
            //    {
            //        Temperature = 0.7f,
            //        TopP = 1,
            //        MaxOutputTokens = 1000,
            //        MaxConversationLength = 50
            //    })
            //};

            
        }
    }
}