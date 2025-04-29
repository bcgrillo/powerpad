using PowerPad.Core.Models.AI;
using PowerPad.WinUI.ViewModels.Agents;
using PowerPad.WinUI.ViewModels.AI;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;

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
                ("OpenAI/gpt-4.1", "https://github.com/marketplace/models/azure-openai/gpt-4-1"),
                ("OpenAI/gpt-4.1-mini", "https://github.com/marketplace/models/azure-openai/gpt-4-1-mini"),
                ("DeepSeek/DeepSeek-R1", "https://github.com/marketplace/models/azureml-deepseek/DeepSeek-R1"),
                ("Meta/Llama-4-Scout-17B-16E-Instruct", "https://github.com/marketplace/models/azureml-meta/Llama-3-3-70B-Instruct"),
                ("Microsoft/MAI-DS-R1", "https://github.com/marketplace/models/azureml/MAI-DS-R1"),
                ("Microsoft/Phi-4", "https://github.com/marketplace/models/azureml/Phi-4")
            ];

            private static readonly (string Name, string Url)[] _initialOpenAIModels =
            [
                ("gpt-4.1", "https://platform.openai.com/docs/models/gpt-4.1"),
                ("gpt-4.1-mini", "https://platform.openai.com/docs/models/gpt-4.1-mini"),
                ("o4-mini", "https://platform.openai.com/docs/models/o4-mini"),
                ("o3-mini", "https://platform.openai.com/docs/models/o3-mini"),
                ("o3", "https://platform.openai.com/docs/models/o3")
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
                AzureAIConfig = new(new() { BaseUrl = "https://models.github.ai/inference" }), //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
                OpenAIConfig = new(new() { BaseUrl = "https://api.openai.com/v1" }), //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

                AvailableProviders = [],

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
                    DefaultModel = new(new("gemma3:4b", ModelProvider.Ollama, "https://ollama.com/library/gemma3", 3338801718), true),
                    DefaultParameters = new(new()
                    {
                        SystemPrompt = "Eres PowerPad, un asistente de inteligencia artificial amable y resolutivo.",
                        Temperature = 0.7f,
                        TopP = 1,
                        MaxOutputTokens = 10000,
                        MaxConversationLength = 50
                    }),
                    SendDefaultParameters = true,
                    AvailableModels = []
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
            }, new("\uE932", AgentIconType.FontIconGlyph))
            {

                Id = Guid.Empty,
                ShowInChats = false
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
            }, new("\uF2B7", AgentIconType.FontIconGlyph))
            {

                Id = new Guid("00000000-0000-0000-0000-000000000001"),
            };

            public readonly static AgentViewModel DefaultAgent3 = new(new()
            {
                Name = "Hazlo más corto",
                Prompt = "Eres experto en resumir textos, haciendo los textos más cortos pero sin omitir nada importante.",
                MaxOutputTokens = 10000,
                Temperature = 0.1f,
                TopP = 1
            }, new("✂️", AgentIconType.CharacterOrEmoji))
            {

                Id = new Guid("00000000-0000-0000-0000-000000000002"),
            };

            public readonly static AgentViewModel DefaultAgent4 = new(new()
            {
                Name = "Poeta",
                Prompt = "Eres un poeta experto en escribir bonitos poemas sobre cualquier cosa."
            }, new("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", AgentIconType.Base64Image))
            {
                Id = new Guid("00000000-0000-0000-0000-000000000003"),
                ShowInNotes = false
            };

            public readonly static ObservableCollection<AgentViewModel> AgentsCollection = 
            [
                DefaultAgent1,
                DefaultAgent2,
                DefaultAgent3,
                DefaultAgent4
            ];
        }

        public static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
}