using PowerPad.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Configuration
{
    public static class ConfigConstants
    {
        public class Keys
        {
            public const string RecentlyWorkspaces = "RecentlyWorkspaces";
            public const string OllamaServiceUrl = "OllamaServiceUrl";
            public const string AzureAIConfig = "AzureAIConfig";
            public const string OpenAIConfig = "OpenAIConfig";
            public const string DefaultModel = "DefaultModel";
        }

        public class Defaults
        {
            public static readonly string WorkspaceFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), nameof(PowerPad));
            public const string OllamaServiceUrl = "http://localhost:11434";
            public static readonly AIModel DefaultModel = new("gemma3:latest", ModelProvider.Ollama);
            public static readonly AzureAIConfig AzureAIConfig = new ("https://models.inference.ai.azure.com", "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");
            public static readonly OpenAIConfig OpenAIConfig = new("https://api.openai.com/v1", "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");
        }
    }
}