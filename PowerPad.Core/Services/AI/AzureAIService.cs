using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.AI;
using PowerPad.Core.Contracts;
using PowerPad.Core.Helpers;
using PowerPad.Core.Models.AI;

namespace PowerPad.Core.Services.AI
{
    /// <summary>  
    /// Implementation of <see cref="IAIService"/> for interacting with Azure AI services.  
    /// Provides methods for initialization, testing connections, managing chat clients, and searching AI models.  
    /// </summary>  
    public class AzureAIService : IAIService
    {
        private const string TEST_MODEL = "openai/gpt-4.1-nano";
        private const int TEST_CONNECTION_TIMEOUT = 5000;

        private ChatCompletionsClient? _azureAI;
        private AIServiceConfig? _config;

        /// <inheritdoc />  
        public void Initialize(AIServiceConfig? config)
        {
            _config = config;
            _azureAI = null;
        }

        /// <inheritdoc />  
        public async Task<TestConnectionResult> TestConnection()
        {
            if (_config is null) return new(ServiceStatus.Unconfigured, "Azure AI service is not initialized.");

            try
            {
                // As of May 2025, there is no way to test the connection without sending a request.
                using var cts = new CancellationTokenSource(TEST_CONNECTION_TIMEOUT);
                await GetClient().AsIChatClient(TEST_MODEL).GetResponseAsync("just put ok", null, cts.Token);

                return new(ServiceStatus.Online);
            }
            catch (OperationCanceledException)
            {
                return new(ServiceStatus.Error, "The operation timed out.");
            }
            catch (Exception ex)
            {
                return new(ServiceStatus.Error, ex.Message.Trim().ReplaceLineEndings(" "));
            }
        }

        /// <inheritdoc />  
        public IChatClient ChatClient(AIModel model, out IEnumerable<string>? notAllowedParameters)
        {
            notAllowedParameters = null;
            return GetClient().AsIChatClient(model.Name);
        }

        /// <inheritdoc />  
        public async Task<IEnumerable<AIModel>> SearchModels(ModelProvider modelProvider, string? query)
        {
            return await GitHubMarketplaceModelsHelper.Search(query);
        }

        /// <summary>  
        /// Retrieves or initializes the Azure AI client.  
        /// </summary>  
        /// <returns>An instance of <see cref="ChatCompletionsClient"/>.</returns>  
        /// <exception cref="InvalidOperationException">Thrown if the service is not initialized or fails to initialize.</exception>  
        private ChatCompletionsClient GetClient()
        {
            if (_config is null) throw new InvalidOperationException("Azure AI service is not initialized.");
            if (_azureAI is not null) return _azureAI;

            try
            {
                _azureAI = new(new(_config.BaseUrl!), new AzureKeyCredential(_config.Key!));
                return _azureAI;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize Azure AI service.", ex);
            }
        }
    }
}