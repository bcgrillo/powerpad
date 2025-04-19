using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Models;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;

namespace PowerPad.Core.Services.AI
{
    public class OpenAIService : IAIService
    {
        private const string GPT_MODEL_PREFIX = "gpt";
        private const string OX_MODEL_PREFIX = "o";
        private const string OPENAI_MODELS_BASE_URL = "https://platform.openai.com/docs/models/";
        private const int TEST_CONNECTION_TIMEOUT = 5000;

        private OpenAIClient? _openAI;
        private AIServiceConfig? _config;

        public void Initialize(AIServiceConfig? config)
        {
            _config = config;
            _openAI = null;
        }

        private OpenAIClient GetClient()
        {
            if (_config is null) throw new InvalidOperationException("OpenAI service is not initialized.");
            if (_openAI is not null) return _openAI;

            try
            {
                _openAI = new(new(_config.Key!), new() { Endpoint = new(_config.BaseUrl!) });
                return _openAI;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize OpenAI service.", ex);
            }
        }

        public async Task<TestConnectionResult> TestConection()
        {
            if (_config is null) return new(ServiceStatus.Unconfigured, "OpenAI service is not initialized.");

            try
            {
                using var cts = new CancellationTokenSource(TEST_CONNECTION_TIMEOUT);
                _ = await GetClient().GetOpenAIModelClient().GetModelsAsync(cts.Token);

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
            var models = await GetClient().GetOpenAIModelClient().GetModelsAsync();

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