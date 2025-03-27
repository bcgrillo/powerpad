using ABI.System;
using Azure.AI.Inference;
using Azure;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OllamaSharp.Models;
using PowerPad.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exception = System.Exception;

namespace PowerPad.Core.Services
{
    public interface IOllamaService
    {
        Task<OllamaStatus> GetStatus();
        Task<IEnumerable<AIModel>> GetAvailableModels();
        IChatClient? ChatClient(AIModel model);
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
            if (_ollama == null) return OllamaStatus.Unknown;

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
            if (_ollama == null) return [];

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
                false,
                model.Size,
                model.Name.Replace("hf.co/", string.Empty).Replace("huggingface.co/", string.Empty)
            );
        }

        public IChatClient? ChatClient(AIModel model)
        {
            if (_ollama == null) return null;

            _ollama.SelectedModel = model.Name;

            return _ollama;
        }
    }
}
