using PowerPad.Core.Models.AI;
using PowerPad.WinUI.ViewModels.Agents;
using PowerPad.WinUI.ViewModels.AI;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            Agents,
        }

        public class StoreDefault
        {
            private static readonly (string Name, string Url)[] _initialGitHubModels =
            [
                ("gpt-4o", "https://github.com/marketplace/models/azure-openai/gpt-4o"),
                ("gpt-4o-mini", "https://github.com/marketplace/models/azure-openai/gpt-4o-mini"),
                ("DeepSeek-R1", "https://github.com/marketplace/models/azureml-deepseek/DeepSeek-R1"),
                ("DeepSeek-V3", "https://github.com/marketplace/models/azureml-deepseek/DeepSeek-V3"),
                ("Llama-3-3-70B-Instruct", "https://github.com/marketplace/models/azureml-meta/Llama-3-3-70B-Instruct"),
                ("Phi-4", "https://github.com/marketplace/models/azureml/Phi-4")
            ];

            private static readonly (string Name, string Url)[] _initialOpenAIModels =
            [
                ("gpt-4o", "https://platform.openai.com/docs/models/gpt-4o"),
                ("gpt-4o-mini", "https://platform.openai.com/docs/models/gpt-4o-mini"),
                ("o3-mini", "https://platform.openai.com/docs/models/o3-mini"),
                ("o1-mini", "https://platform.openai.com/docs/models/o1-mini"),
                ("o1", "https://platform.openai.com/docs/models/o1")
            ];

            public static readonly string WorkspaceFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), nameof(PowerPad));

            public static readonly GeneralSettingsViewModel GeneralSettings = new()
            {
                OllamaEnabled = true,
                AzureAIEnabled = false,
                OpenAIEnabled = false,

                OllamaConfig = new(new() { BaseUrl = "http://localhost:11434" }),
                OllamaAutostart = true,
                //TODO: Remove
                AzureAIConfig = new(new() { BaseUrl = "https://models.inference.ai.azure.com", Key = "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" }),
                OpenAIConfig = new(new() { BaseUrl = "https://api.openai.com/v1", Key = "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" }),

                AppTheme = null, //Use system configuration
                AcrylicBackground = true,

                AgentPrompt =
                    $"IMPORTANTE:\n" +
                    $"- El mensaje de usuario recibido es tu información de entrada.\n" +
                    $"- Si necesitas realizar una modificación, tu respuesta debe ser el texto completo modificado, manteniendo intacto lo que no sea necesario cambiar.\n" +
                    $"- No incluyas explicaciones, saludos ni mensajes adicionales, solamente el texto solicitado.",
        };

            public static ModelsSettingsViewModel GenerateDefaultModelsSettings()
            {
                var defaultModelSettings = new ModelsSettingsViewModel
                {
                    DefaultModel = new(new("gemma3:latest", ModelProvider.Ollama, "https://ollama.com/library/gemma3", 3338801718), true),
                    DefaultParameters = new(new()
                    {
                        SystemPrompt = "Eres PowerPad, un asistente de inteligencia artificial amable y resolutivo.",
                        Temperature = 0.7f,
                        TopP = 1,
                        MaxOutputTokens = 1000,
                        MaxConversationLength = 50
                    }),
                    SendDefaultParameters = true,
                    AvailableModels = [],
                    RecoverableModels = []
                };

                defaultModelSettings.AvailableModels.Add(defaultModelSettings.DefaultModel);
                defaultModelSettings.AvailableModels.AddRange(_initialGitHubModels.Select(m => new AIModelViewModel(new(m.Name, ModelProvider.GitHub, m.Url), true)));
                defaultModelSettings.AvailableModels.AddRange(_initialOpenAIModels.Select(m => new AIModelViewModel(new(m.Name, ModelProvider.OpenAI, m.Url), true)));

                return defaultModelSettings;
            }

            public readonly static AgentViewModel DefaultAgent1 = new(new()
            {
                Name = "PowerEditor",
                Prompt = "Eres un editor de texto que cumples la acción solicitada por el usuario.",
                PromptParameterName = "Acción",
                PromptParameterDescription = "¿Qué quieres hacer?",
                MaxOutputTokens = 10000,
                Temperature = 0.1f,
                TopP = 1
            })
            {
                AgentIcon = new AgentIcon
                {
                    IconType = AgentIconType.FontIconGlyph,
                    IconSource = "\uE932",
                },
                Enabled = true
            };

            public readonly static AgentViewModel DefaultAgent2 = new(new()
            {
                Name = "Traductor",
                Prompt = "Eres un traductor de texto que traduces todo al idioma solicitado por el usuario.",
                PromptParameterName = "Idioma",
                PromptParameterDescription = "Idioma de destino",
                MaxOutputTokens = 10000,
                Temperature = 0.1f,
                TopP = 1
            })
            {
                AgentIcon = new AgentIcon
                {
                    IconType = AgentIconType.FontIconGlyph,
                    IconSource = "\uF2B7",
                },
                Enabled = true
            };

            public readonly static AgentViewModel DefaultAgent3 = new(new()
            {
                Name = "Hazlo más corto",
                Prompt = "Eres experto en resumir textos, haciendo los textos más cortos pero sin omitir nada importante.",
                MaxOutputTokens = 10000,
                Temperature = 0.1f,
                TopP = 1
            })
            {
                AgentIcon = new AgentIcon
                {
                    IconType = AgentIconType.CharacterOrEmoji,
                    IconSource = "✂️",
                },
                Enabled = true
            };

            public readonly static AgentViewModel DefaultAgent4 = new(new()
            {
                Name = "Poeta",
                Prompt = "Eres un poeta que conviertes cualquier texto en un bonito poema.",
                MaxOutputTokens = 10000,
                Temperature = 1f,
                TopP = 1
            })
            {
                AgentIcon = new AgentIcon
                {
                    IconType = AgentIconType.Base64Image,
                    IconSource = "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",
                },
                Enabled = true
            };

            public readonly static ObservableCollection<AgentViewModel> AgentsCollection = 
            [
                DefaultAgent1,
                DefaultAgent2,
                DefaultAgent3,
                DefaultAgent4
            ];
        }
    }
}