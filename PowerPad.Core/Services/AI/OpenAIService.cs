using Microsoft.Extensions.AI;
using OllamaSharp.Models;
using OpenAI;
using OpenAI.Models;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;
using System.ClientModel;
using System.Collections.ObjectModel;
using System.Globalization;
using Uri = System.Uri;

namespace PowerPad.Core.Services.AI
{
    public interface IOpenAIService : IAIService
    {
    }

    public class OpenAIService : IOpenAIService
    {
        private OpenAIClient? _openAI;

        public void Initialize(AIServiceConfig config)
        {
            ArgumentException.ThrowIfNullOrEmpty(config.BaseUrl);
            ArgumentException.ThrowIfNullOrEmpty(config.Key);

            _openAI = new OpenAIClient(new ApiKeyCredential(config.Key), new OpenAIClientOptions { Endpoint = new Uri(config.BaseUrl) });
        }

        public IChatClient? ChatClient(AIModel model) => _openAI?.AsChatClient(model.Name);

        public async Task<IEnumerable<AIModel>> SearchModels(ModelProvider modelProvider, string? query)
        {
            if (_openAI is null) return [];

            var models = await _openAI.GetOpenAIModelClient().GetModelsAsync();

            return string.IsNullOrEmpty(query)
                ? models.Value.Select(CreateAIModel)
                : models.Value.Where(m => m.Id.Contains(query)).Select(CreateAIModel);
        }

        private static AIModel CreateAIModel(OpenAIModel openAIModel)
        {
            return new AIModel(openAIModel.Id, ModelProvider.OpenAI);
        }
    }
}
