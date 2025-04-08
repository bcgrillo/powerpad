using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.AI;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;

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

            _azureAI = new(new(config.BaseUrl), new AzureKeyCredential(config.Key));
        }

        public async Task<TestConnectionResult> TestConection()
        {
            if (_azureAI is null) return new(false, "Azure AI is not initialized.");

            try
            {
                var result = await _azureAI.AsChatClient(string.Empty).GetResponseAsync("test");

                return new(true);
            }
            catch (Exception ex)
            {
                return new(false, ex.Message);
            }
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
                new(query ?? "xxx", modelProvider),
                new("yyy", modelProvider),
                new("zzz", modelProvider),
            ]);
        }
    }
}
