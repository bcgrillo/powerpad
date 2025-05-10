using PowerPad.Core.Models.AI;
using PowerPad.WinUI.ViewModels.Agents;
using PowerPad.WinUI.ViewModels.AI;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

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
                    $"- No incluyas explicaciones, saludos ni mensajes adicionales, solamente el texto requerido.",

                EnableHotKeys = true
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
            }, new("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", AgentIconType.Base64Image))
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
            }, new("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", AgentIconType.Base64Image))
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
            }, new("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", AgentIconType.Base64Image))
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