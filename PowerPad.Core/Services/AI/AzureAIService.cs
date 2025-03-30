using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.AI;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;
using Uri = System.Uri;

namespace PowerPad.Core.Services.AI
{
    public interface IAzureAIService : IAIService
    {
    }

    public class AzureAIService : IAzureAIService
    {
        private ChatCompletionsClient? _azureAI;

        public void Initialize(AIServiceConfig config)
        {
            ArgumentException.ThrowIfNullOrEmpty(config.BaseUrl);
            ArgumentException.ThrowIfNullOrEmpty(config.Key);

            _azureAI = new ChatCompletionsClient(new Uri(config.BaseUrl), new AzureKeyCredential(config.Key));
        }

        public async Task<IEnumerable<AIModel>> GetAvailableModels()
        {
            return await Task.FromResult<IEnumerable<AIModel>>([]);
        }

        public IChatClient? ChatClient(AIModel model) => _azureAI?.AsChatClient(model.Name);

        public Task<IEnumerable<AIModel>> SearchModels(ModelProvider modelProvider, string? query)
        {
            //TODO: Implement search models

            return Task.FromResult<IEnumerable<AIModel>>(
            [
                new AIModel(query ?? "xxx", modelProvider),
                new AIModel("yyy", modelProvider),
                new AIModel("zzz", modelProvider),
            ]);
        }
    }
}
