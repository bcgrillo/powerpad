using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.AI;
using OllamaSharp;
using PowerPad.Core.Contracts;
using PowerPad.Core.Helpers;
using PowerPad.Core.Models.AI;

namespace PowerPad.Core.Services.AI
{
    public interface IAzureAIService : IAIService
    {
    }

    public class AzureAIService : IAzureAIService
    {
        private const string TEST_MODEL = "gpt-4o";

        private ChatCompletionsClient? _azureAI;
        private AIServiceConfig? _config;

        public void Initialize(AIServiceConfig config)
        {
            _config = !string.IsNullOrEmpty(config.BaseUrl) && !string.IsNullOrEmpty(config.Key) ? config : null;
            _azureAI = null;
        }

        public ChatCompletionsClient? GetClient()
        {
            if (_azureAI is not null) return _azureAI;
            if (_config is null) return null;

            _azureAI = new(new(_config.BaseUrl!), new AzureKeyCredential(_config.Key!));
            return _azureAI;
        }

        public async Task<TestConnectionResult> TestConection()
        {
            if (_config is null) return new(false, "Azure AI is not initialized.");

            try
            {
                //TODO: Check a better way to do this
                var result = await GetClient()!.AsChatClient(TEST_MODEL).GetResponseAsync("test");

                return new(true);
            }
            catch (Exception ex)
            {
                return new(false, ex.Message);
            }
        }

        public IChatClient? ChatClient(AIModel model) => GetClient()?.AsChatClient(model.Name);

        public async Task<IEnumerable<AIModel>> SearchModels(ModelProvider modelProvider, string? query)
        {
            return await GitHubMarktplaceModelsHelper.Search(query);
        }
    }
}