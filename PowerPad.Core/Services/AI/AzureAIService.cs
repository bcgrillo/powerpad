using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.AI;
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

        public void Initialize(AIServiceConfig config)
        {
            ArgumentException.ThrowIfNullOrEmpty(config.BaseUrl);
            ArgumentException.ThrowIfNullOrEmpty(config.Key);

            _azureAI = new(new(config.BaseUrl), new AzureKeyCredential(config.Key));
        }

        public async Task<TestConnectionResult> TestConection()
        {
            if (_azureAI is null) return new(false, "Azure AI is not initialized.");

            try
            {
                //TODO: Check a better way to do this
                var result = await _azureAI.AsChatClient(TEST_MODEL).GetResponseAsync("test");

                return new(true);
            }
            catch (Exception ex)
            {
                return new(false, ex.Message);
            }
        }

        public IChatClient? ChatClient(AIModel model) => _azureAI?.AsChatClient(model.Name);

        public async Task<IEnumerable<AIModel>> SearchModels(ModelProvider modelProvider, string? query)
        {
            return await GitHubMarktplaceModelsHelper.Search(query);
        }
    }
}