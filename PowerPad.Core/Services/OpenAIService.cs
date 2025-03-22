using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Models;
using PowerPad.Core.Models;
using System.ClientModel;
using Uri = System.Uri;

namespace PowerPad.Core.Services
{
    public interface IOpenAIService
    {
        Task<IEnumerable<AIModel>> GetModels();
        IChatClient ChatClient(AIModel model);
    }

    public class OpenAIService : IOpenAIService
    {
        private readonly OpenAIClient? _openAI;

        public OpenAIService(string baseUrl, string key)
        {
            _openAI = new OpenAIClient(new ApiKeyCredential(key), 
                new OpenAIClientOptions { Endpoint = new Uri(baseUrl) });
        }

        public async Task<IEnumerable<AIModel>> GetModels()
        {
            if (_openAI == null) return [];

            var models = await _openAI.GetOpenAIModelClient().GetModelsAsync();

            return models.Value.Select(m => CreateAIModel(m));
        }

        private static AIModel CreateAIModel(OpenAIModel openAIModel)
        {
            return new AIModel(
                openAIModel.Id,
                ModelProvider.OpenAI,
                Status: ModelStatus.Available
            );
        }

        public IChatClient ChatClient(AIModel model)
        {
            if (_openAI == null) throw new InvalidOperationException("Open AI service not initialized.");

            return _openAI.AsChatClient(model.Name);
        }
    }
}
