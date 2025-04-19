using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.AI;
using PowerPad.Core.Contracts;
using PowerPad.Core.Helpers;
using PowerPad.Core.Models.AI;

namespace PowerPad.Core.Services.AI
{
    public class AzureAIService : IAIService
    {
        private const string TEST_MODEL = "gpt-4o-mini";
        private const int TEST_CONNECTION_TIMEOUT = 2000;

        private ChatCompletionsClient? _azureAI;
        private AIServiceConfig? _config;

        public void Initialize(AIServiceConfig? config)
        {
            _config = config;
            _azureAI = null;
        }

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

        public async Task<TestConnectionResult> TestConection()
        {
            if (_config is null) return new(ServiceStatus.Unconfigured, "Azure AI service is not initialized.");

            try
            {
                //TODO: Check a better way to do this
                using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(TEST_CONNECTION_TIMEOUT));
                var result = await GetClient().AsChatClient(TEST_MODEL).GetResponseAsync("test", null, cts.Token);

                return new(ServiceStatus.Online);
            }
            catch (OperationCanceledException)
            {
                return new(ServiceStatus.Error, "The operation timed out.");
            }
            catch (Exception ex)
            {
                return new(ServiceStatus.Error, ex.Message);
            }
        }

        public IChatClient ChatClient(AIModel model) => GetClient().AsChatClient(model.Name);

        public async Task<IEnumerable<AIModel>> SearchModels(ModelProvider modelProvider, string? query)
        {
            return await GitHubMarktplaceModelsHelper.Search(query);
        }
    }
}