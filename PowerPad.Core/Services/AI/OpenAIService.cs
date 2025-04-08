using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Models;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;

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

            _openAI = new(new(config.Key), new() { Endpoint = new(config.BaseUrl) });
        }

        public async Task<TestConnectionResult> TestConection()
        {
            if (_openAI is null) return new(false, "OpenAI service is not initialized.");

            try
            {
                _ = await _openAI.GetOpenAIModelClient().GetModelsAsync();

                return new(true);
            }
            catch (Exception ex)
            {
                return new(false, ex.Message);
            }
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
            return new(openAIModel.Id, ModelProvider.OpenAI);
        }
    }
}
