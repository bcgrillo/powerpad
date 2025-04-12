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
        private const string GPT_MODEL_PREFIX = "gpt";
        private const string OX_MODEL_PREFIX = "o";
        private const string OPENAI_MODELS_BASE_URL = "https://platform.openai.com/docs/models/";

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

            // Compatible models are those that start with "gpt" or "oX" where X is a digit
            // By now this is the only way to filter models compatible with chat completions
            var compatibleModels = models.Value.Where(m =>
                m.Id.StartsWith(GPT_MODEL_PREFIX, StringComparison.InvariantCultureIgnoreCase) ||
                (m.Id.Length > 1 && m.Id[0] == OX_MODEL_PREFIX[0] && char.IsDigit(m.Id[1]))
            ).OrderByDescending(m => m.CreatedAt);

            return string.IsNullOrEmpty(query)
                ? compatibleModels.Select(CreateAIModel)
                : compatibleModels.Where(m => m.Id.Contains(query)).Select(CreateAIModel);
        }

        private static AIModel CreateAIModel(OpenAIModel openAIModel)
        {
            string modelId = openAIModel.Id;
            string infoUrl = $"{OPENAI_MODELS_BASE_URL}{modelId}";

            if (modelId.Length > 10 && DateTime.TryParseExact(modelId[^10..], "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out _))
            {
                infoUrl = $"{OPENAI_MODELS_BASE_URL}{modelId[..^11]}";
            }

            return new
            (
                modelId,
                ModelProvider.OpenAI,
                infoUrl
            );
        }
    }
}