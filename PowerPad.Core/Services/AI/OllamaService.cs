using Microsoft.Extensions.AI;
using OllamaSharp;
using OllamaSharp.Models;
using System.Diagnostics;
using Exception = System.Exception;
using PowerPad.Core.Models.AI;
using PowerPad.Core.Contracts;

namespace PowerPad.Core.Services.AI
{
    public interface IOllamaService : IAIService
    {
        Task<OllamaStatus> GetStatus();
        Task<IEnumerable<AIModel>> GetAvailableModels();
        Task Start();
        Task Stop();
    }

    public class OllamaService : IOllamaService
    {
        private OllamaApiClient? _ollama;
        private AIServiceConfig? _config;

        public void Initialize(AIServiceConfig config)
        {
            ArgumentException.ThrowIfNullOrEmpty(config.BaseUrl);

            _config = config;
            _ollama = null;
        }

        public OllamaApiClient? GetClient()
        {
            if (_ollama is not null) return _ollama;
            if (_config is null) return null;

            _ollama = new(_config.BaseUrl!);
            return _ollama;
        }

        public async Task<TestConnectionResult> TestConection()
        {
            if (_config is null) return new(false, "Ollama is not initialized.");

            try
            {
                var result = await GetClient()!.IsRunningAsync();

                return new(result, result ? null : "Ollama is not running.");
            }
            catch (Exception ex)
            {
                return new(false, ex.Message);
            }
        }

        public async Task<OllamaStatus> GetStatus()
        {
            bool connected = false;

            if (_config is not null)
            {
                try
                {
                    connected = await GetClient()!.IsRunningAsync();
                }
                catch
                {
                }

                if (connected)
                {
                    return OllamaStatus.Online;
                }
                else
                {
                    try
                    {
                        if (GetProcesses().Any())
                        {
                            return OllamaStatus.Unreachable;
                        }
                        else
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
                    }
                    catch
                    {
                        return OllamaStatus.Error;
                    }
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
                provider == ModelProvider.HuggingFace
                    ? model.Name.Replace("hf.co/", string.Empty).Replace("huggingface.co/", string.Empty)
                    : null
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
                new(query ?? "xxx", modelProvider),
                new("yyy", modelProvider),
                new("zzz", modelProvider),
            ];
        }

        public Task Start()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "ollama app.exe",
                UseShellExecute = true,
                CreateNoWindow = true
            };

            Process.Start(startInfo);

            return Task.CompletedTask;  
        }

        public async Task Stop()
        {
            foreach (var process in GetProcesses())
            {
                process.Kill();
                await process.WaitForExitAsync();
            }
        }

        private static IEnumerable<Process> GetProcesses()
        {
            List<string> processesName = ["ollama app", "ollama"];

            foreach (var processName in processesName)
            {
                foreach (var process in Process.GetProcessesByName(processName))
                {
                    yield return process;
                }
            }
        }
    }
}