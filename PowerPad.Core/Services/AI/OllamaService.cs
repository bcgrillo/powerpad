using Microsoft.Extensions.AI;
using OllamaSharp;
using OllamaSharp.Models;
using System.Diagnostics;
using Exception = System.Exception;
using PowerPad.Core.Models.AI;
using PowerPad.Core.Contracts;
using PowerPad.Core.Helpers;
using System.ComponentModel;

namespace PowerPad.Core.Services.AI
{
    public interface IOllamaService
    {
        Task<IEnumerable<AIModel>> GetInstalledModels();
        Task Start();
        Task Stop();
        Task DownloadModel(AIModel model, Action<double> updateAction, Action<Exception> errorAction, CancellationToken cancellationToken);
        Task DeleteModel(AIModel model);
    }

    public class OllamaService : IAIService, IOllamaService
    {
        private const string HF_OLLAMA_PREFIX = "hf.co/";
        private const string HF_OLLAMA_PREFIX_AUX = "huggingface.co/";

        private const int TEST_CONNECTION_TIMEOUT = 5000;
        private const int DELAY_AFTER_START = 500;
        private const int DOWNLOAD_UPDATE_INTERVAL = 200;

        private OllamaApiClient? _ollama;
        private AIServiceConfig? _config;

        public void Initialize(AIServiceConfig? config)
        {
            _config = config;
            _ollama = null;
        }

        private OllamaApiClient GetClient()
        {
            if (_config is null) throw new InvalidOperationException("Ollama is not initialized.");
            if (_ollama is not null) return _ollama;

            try
            {
                _ollama = new(_config.BaseUrl!);
                return _ollama;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize Ollama.", ex);
            }
        }

        public async Task<TestConnectionResult> TestConection()
        {
            if (_config is null) return new(ServiceStatus.Unconfigured, "Ollama is not initialized.");

            bool connected = false;

            try
            {
                using var cts = new CancellationTokenSource(TEST_CONNECTION_TIMEOUT);
                connected = await GetClient().IsRunningAsync(cts.Token);
            }
            catch
            {
            }

            if (connected)
            {
                return new(ServiceStatus.Online);
            }
            else
            {
                try
                {
                    if (GetProcesses().Any())
                    {
                        return new(ServiceStatus.Error, "Ollama is running, but not reachable.");
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

                        if (process.ExitCode == 0) return new(ServiceStatus.Available);
                        else return new(ServiceStatus.Error, $"Ollama error: {process.StandardError.ReadToEnd()}");
                    }
                }
                catch (Exception ex)
                {
                    if (ex is Win32Exception)
                        return new(ServiceStatus.NotFound, "Ollama not found.");
                    else
                        return new(ServiceStatus.Error, $"Ollama error: {ex.Message.Trim().ReplaceLineEndings(" ")}");
                }
            }
        }

        public async Task<IEnumerable<AIModel>> GetInstalledModels()
        {
            var models = await GetClient().ListLocalModelsAsync();

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

        public IChatClient ChatClient(AIModel model, out IEnumerable<string> notAllowedParameters)
        {
            var client = GetClient();

            client.SelectedModel = model.Name;
            notAllowedParameters = [];

            return client;
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

        public async Task Start()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "ollama app.exe",
                UseShellExecute = true,
                CreateNoWindow = true
            };

            Process.Start(startInfo);

            await Task.Delay(DELAY_AFTER_START);
        }

        public async Task Stop()
        {
            foreach (var process in GetProcesses())
            {
                process.Kill();
                await process.WaitForExitAsync();
            }
        }

        public async Task DownloadModel(AIModel model, Action<double> updateAction, Action<Exception> errorAction, CancellationToken cancellationToken)
        {
            if (_config is null)
            {
                errorAction(new Exception("Ollama is not initialized."));
            }
            else
            {
                try
                {
                    await foreach (var status in GetClient()!.PullModelAsync(model.Name, cancellationToken))
                    {
                        var progress = Math.Clamp(status?.Percent ?? 0.0D, 0, 99.5);
                        
                        updateAction(progress);

                        await Task.Delay(DOWNLOAD_UPDATE_INTERVAL, cancellationToken);
                    }

                    updateAction(100);
                }
                catch (Exception ex)
                {
                    errorAction(ex);
                }
            }
        }

        public async Task DeleteModel(AIModel model)
        {
            if (_config is null) return;

            //TODO: Error si no se ha descargado aun
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