using PowerPad.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
        }

        public class StoreDefault
        {
            private static readonly string[] _initialGitHubModels =
                ["gpt-4o", "gpt-4o-mini", "DeepSeek-R1", "DeepSeek-V3", "Llama-3-3-70B-Instruct", "Phi-4"];
            private static readonly string[] _initialOpenAIModels =
                ["gpt-4o", "gpt-4o-mini", "o3-mini", "o1-mini", "o1"];

            public static readonly string WorkspaceFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), nameof(PowerPad));
            
            public static readonly GeneralSettings GeneralSettings = new()
            {
                OllamaEnabled = true,
                AzureAIEnabled = false,
                OpenAIEnabled = false,

                OllamaConfig = new() { BaseUrl = "http://localhost:11434" },
                //TODO: Remove
                AzureAIConfig = new() { BaseUrl = "https://models.inference.ai.azure.com", Key = "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" },
                OpenAIConfig = new() { BaseUrl = "https://api.openai.com/v1", Key = "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" },
                
                AppTheme = null, //Use system configuration
                AcrylicBackground = true,
            };

            public static readonly ModelsSettings ModelsSettings = new()
            {
                DefaultModel = new("gemma3:latest", ModelProvider.Ollama, true, 1000),

                AvailableModels =
                [
                    new("gemma3:latest", ModelProvider.Ollama, true, 1000),
                    .. _initialGitHubModels.Select(m => new AIModel(m, ModelProvider.GitHub)),
                    .. _initialOpenAIModels.Select(m => new AIModel(m, ModelProvider.GitHub))
                ]
            };
        }
    }
}