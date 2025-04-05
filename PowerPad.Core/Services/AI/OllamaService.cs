using ABI.System;
using Azure.AI.Inference;
using Azure;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OllamaSharp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exception = System.Exception;
using PowerPad.Core.Models.AI;
using PowerPad.Core.Models.Config;
using PowerPad.Core.Contracts;

namespace PowerPad.Core.Services.AI
{
    public interface IOllamaService : IAIService
    {
        Task<OllamaStatus> GetStatus();
        Task<IEnumerable<AIModel>> GetAvailableModels();
    }

    public class OllamaService : IOllamaService
    {
        private OllamaApiClient? _ollama;

        public void Initialize(AIServiceConfig config)
        {
            ArgumentException.ThrowIfNullOrEmpty(config.BaseUrl);

            _ollama = new OllamaApiClient(config.BaseUrl);
        }

        public async Task<OllamaStatus> GetStatus()
        {
            if (_ollama is null) return OllamaStatus.Unknown;

            bool connected;

            try
            {
                connected = await _ollama.IsRunningAsync();
            }
            catch (Exception)
            {
                connected = false;
            }

            if (connected)
            {
                return OllamaStatus.Online;
            }
            else
            {
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "ollama",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = Process.Start(startInfo)!;

                    await process.WaitForExitAsync();

                    if (process.ExitCode == 0) return OllamaStatus.Available;
                }
                catch (Exception)
                {
                    //TODO: Something
                }
            }

            return OllamaStatus.Unknown;
        }

        public async Task<IEnumerable<AIModel>> GetAvailableModels()
        {
            if (_ollama is null) return [];

            var models = await _ollama.ListLocalModelsAsync();

            return models.Select(m => CreateAIModel(m));
        }

        private static AIModel CreateAIModel(Model model)
        {
            ModelProvider provider;

            if (model.Name.StartsWith("hf.co") || model.Name.StartsWith("huggingface.co"))
                provider = ModelProvider.HuggingFace;
            else
                provider = ModelProvider.Ollama;

            return new AIModel
            (
                model.Name, 
                provider,
                model.Size,
                model.Name.Replace("hf.co/", string.Empty).Replace("huggingface.co/", string.Empty)
            );
        }

        public IChatClient? ChatClient(AIModel model)
        {
            if (_ollama is null) return null;

            _ollama.SelectedModel = model.Name;

            return _ollama;
        }

        public async Task<IEnumerable<AIModel>> SearchModels(ModelProvider modelProvider, string? query)
        {
            //TODO: Implement search models
            //Remember change the name of huggingface models, and set displayname (see CreateAIModel method)
            
            await Task.Delay(2000);

            return [
                new AIModel(query ?? "xxx", modelProvider),
                new AIModel("yyy", modelProvider),
                new AIModel("zzz", modelProvider),
            ];
        }
    }
}
