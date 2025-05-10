using Microsoft.Extensions.AI;
using OllamaSharp;
using OllamaSharp.Models;
using PowerPad.Core.Contracts;
using PowerPad.Core.Helpers;
using PowerPad.Core.Models.AI;
using System.ComponentModel;
using System.Diagnostics;

namespace PowerPad.Core.Services.AI
{
    /// <summary>  
    /// Defines the contract for managing Ollama AI services, including model management and service lifecycle operations.  
    /// </summary>  
    public interface IOllamaService
    {
        /// <summary>  
        /// Retrieves the list of installed AI models.  
        /// </summary>  
        /// <returns>A collection of <see cref="AIModel"/> representing the installed models.</returns>  
        Task<IEnumerable<AIModel>> GetInstalledModels();

        /// <summary>  
        /// Starts the Ollama service.  
        /// </summary>  
        Task Start();

        /// <summary>  
        /// Stops the Ollama service.  
        /// </summary>  
        Task Stop();

        /// <summary>  
        /// Downloads an AI model with progress updates and error handling.  
        /// </summary>  
        /// <param name="model">The AI model to download.</param>  
        /// <param name="updateAction">An action to report download progress.</param>  
        /// <param name="errorAction">An action to handle errors during the download.</param>  
        /// <param name="cancellationToken">A token to cancel the download operation.</param>  
        Task DownloadModel(AIModel model, Action<double> updateAction, Action<Exception> errorAction, CancellationToken cancellationToken);

        /// <summary>  
        /// Deletes an installed AI model.  
        /// </summary>  
        /// <param name="model">The AI model to delete.</param>  
        Task DeleteModel(AIModel model);
    }

    /// <summary>  
    /// Provides an implementation of <see cref="IAIService"/> and <see cref="IOllamaService"/> for managing Ollama AI services.  
    /// </summary>  
    public class OllamaService : IAIService, IOllamaService
    {
        private const string HF_OLLAMA_PREFIX = "hf.co/";
        private const string HF_OLLAMA_PREFIX_AUX = "huggingface.co/";

        private const int TEST_CONNECTION_TIMEOUT = 5000;
        private const int DELAY_AFTER_START = 500;
        private const int DOWNLOAD_UPDATE_INTERVAL = 200;

        private OllamaApiClient? _ollama;
        private AIServiceConfig? _config;

        /// <inheritdoc />  
        public void Initialize(AIServiceConfig? config)
        {
            _config = config;
            _ollama = null;
        }

        /// <inheritdoc />  
        public async Task<TestConnectionResult> TestConnection()
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
                // Connection test failed, but we can still check if the process is running
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
                        else return new(ServiceStatus.Error, $"Ollama error: {await process.StandardError.ReadToEndAsync()}");
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

        /// <inheritdoc />  
        public async Task<IEnumerable<AIModel>> GetInstalledModels()
        {
            var models = await GetClient().ListLocalModelsAsync();

            return models.Select(m => CreateAIModel(m));
        }

        /// <inheritdoc />  
        public IChatClient ChatClient(AIModel model, out IEnumerable<string>? notAllowedParameters)
        {
            var client = GetClient();

            client.SelectedModel = model.Name;
            notAllowedParameters = null;

            return client;
        }

        /// <inheritdoc />  
        public async Task<IEnumerable<AIModel>> SearchModels(ModelProvider modelProvider, string? query)
        {
            return modelProvider switch
            {
                ModelProvider.Ollama => await OllamaLibraryHelper.Search(query),
                ModelProvider.HuggingFace => await HuggingFaceLibraryHelper.Search(query),
                _ => throw new NotImplementedException($"Model provider {modelProvider} is not implemented in OllamaService Search.")
            };
        }

        /// <inheritdoc />  
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

        /// <inheritdoc />  
        public async Task Stop()
        {
            foreach (var process in GetProcesses())
            {
                process.Kill();
                await process.WaitForExitAsync();
            }
        }

        /// <inheritdoc />  
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

        /// <inheritdoc />  
        public async Task DeleteModel(AIModel model)
        {
            if (_config is null) return;

            //TODO: Error si no se ha descargado aun  
            await GetClient()!.DeleteModelAsync(model.Name);
        }

        /// <summary>  
        /// Retrieves the Ollama API client, initializing it if necessary.  
        /// </summary>  
        /// <returns>An instance of <see cref="OllamaApiClient"/>.</returns>  
        /// <exception cref="InvalidOperationException">Thrown if the service is not initialized or fails to initialize.</exception>  
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

        /// <summary>  
        /// Creates an <see cref="AIModel"/> instance from a <see cref="Model"/>.  
        /// </summary>  
        /// <param name="model">The source model.</param>  
        /// <returns>An instance of <see cref="AIModel"/>.</returns>  
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

        /// <summary>  
        /// Retrieves the list of running Ollama processes.  
        /// </summary>  
        /// <returns>An enumerable of <see cref="Process"/> instances representing the running processes.</returns>  
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