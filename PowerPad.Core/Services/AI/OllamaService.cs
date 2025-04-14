using Microsoft.Extensions.AI;
using OllamaSharp;
using OllamaSharp.Models;
using System.Diagnostics;
using Exception = System.Exception;
using PowerPad.Core.Models.AI;
using PowerPad.Core.Contracts;
using PowerPad.Core.Helpers;

namespace PowerPad.Core.Services.AI
{
    public interface IOllamaService : IAIService
    {
        Task<OllamaStatus> GetStatus();
        Task<IEnumerable<AIModel>> GetAvailableModels();
        Task Start();
        Task Stop();
        Task Download(AIModel model, Action<double> updateAction, Action<Exception> errorAction);
        Task RemoveModel(AIModel model);
    }

    public class OllamaService : IOllamaService
    {
        private const string HF_OLLAMA_PREFIX = "hf.co";
        private const string HF_OLLAMA_PREFIX_AUX = "huggingface.co";

        private const int DOWNLOAD_UPDATE_INTERVAL = 200;

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

            if (model.Name.StartsWith(HF_OLLAMA_PREFIX) || model.Name.StartsWith(HF_OLLAMA_PREFIX_AUX))
                provider = ModelProvider.HuggingFace;
            else
                provider = ModelProvider.Ollama;

            return new AIModel
            (
                model.Name,
                provider,
                provider == ModelProvider.HuggingFace
                    ? HuggingFaceLibraryHelper.GetModelUrl(model.Name)
                    : OllamaLibraryHelper.GetModelUrl(model.Name),
                model.Size,
                provider == ModelProvider.HuggingFace
                    ? model.Name.Replace(HF_OLLAMA_PREFIX, string.Empty).Replace(HF_OLLAMA_PREFIX_AUX, string.Empty)
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
            return modelProvider switch
            {
                ModelProvider.Ollama => await OllamaLibraryHelper.Search(query),
                ModelProvider.HuggingFace => await HuggingFaceLibraryHelper.Search(query),
                _ => throw new NotImplementedException($"Model provider {modelProvider} is not implemented in OllamaService Search.")
            };
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

        public async Task Download(AIModel model, Action<double> updateAction, Action<Exception> errorAction)
        {
            if (_config is null)
            {
                errorAction(new Exception("Ollama is not initialized."));
            }
            else
            {
                try
                {
                    await foreach (var status in GetClient()!.PullModelAsync(model.Name))
                    {
                        var progress = Math.Clamp(status?.Percent ?? 0.0D, 0, 99.5);
                        
                        updateAction(progress);

                        await Task.Delay(DOWNLOAD_UPDATE_INTERVAL);
                    }

                    updateAction(100);
                }
                catch (Exception ex)
                {
                    errorAction(ex);
                }
            }
        }

        public async Task RemoveModel(AIModel model)
        {
            if (_ollama is null) return;

            await GetClient()!.DeleteModelAsync(model.Name);
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