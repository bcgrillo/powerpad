using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.AI;
using OpenAI;
using PowerPad.Core.Models;
using System.ClientModel;
using System.Collections.ObjectModel;
using Uri = System.Uri;

namespace PowerPad.Core.Services
{
    public interface IAzureAIService
    {
        Task<IEnumerable<AIModel>> GetAvaliableModels();
        IChatClient? ChatClient(AIModel model);
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

        public async Task<IEnumerable<AIModel>> GetAvaliableModels()
        {
            return await Task.FromResult<IEnumerable<AIModel>>([]);
        }

        public IChatClient? ChatClient(AIModel model) => _azureAI?.AsChatClient(model.Name);
    }
}
