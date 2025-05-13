using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Models;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PowerPad.Core.Services.AI
{
    /// <summary>
    /// Implementation of <see cref="IAIService"/> for interacting with OpenAI's API.
    /// Provides methods for initializing the service, testing connections, managing chat clients, and searching AI models.
    /// </summary>
    public class OpenAIService : IAIService
    {
        private const string GPT_MODEL_PREFIX = "gpt";
        private const string OX_MODEL_PREFIX = "o";
        private static readonly string[] EXCLUDED_WORDS = ["realtime", "image", "audio", "search", "transcribe"];
        private const string OPENAI_MODELS_BASE_URL = "https://platform.openai.com/docs/models/";
        private const int TEST_CONNECTION_TIMEOUT = 5000;
        private static readonly string[] REASONING_MODELS_NOT_ALLOWED_PARAMETERS =
        [
            nameof(IChatOptions.Temperature),
            nameof(IChatOptions.TopP)
        ];

        private OpenAIClient? _openAI;
        private AIServiceConfig? _config;

        /// <inheritdoc />
        public void Initialize(AIServiceConfig? config)
        {
            _config = config;
            _openAI = null;
        }

        /// <inheritdoc />
        public async Task<TestConnectionResult> TestConnection()
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
                return new(ServiceStatus.Error, ex.Message.Trim().ReplaceLineEndings(" "));
            }
        }

        /// <inheritdoc />
        public IChatClient ChatClient(AIModel model, out IEnumerable<string>? notAllowedParameters)
        {
            var isReasoningModel = model.Name[0] == OX_MODEL_PREFIX[0] && char.IsDigit(model.Name[1]);

            notAllowedParameters = isReasoningModel ? REASONING_MODELS_NOT_ALLOWED_PARAMETERS : null;

            return GetClient().GetChatClient(model.Name).AsIChatClient();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AIModel>> SearchModels(ModelProvider modelProvider, string? query)
        {
            var models = await GetClient().GetOpenAIModelClient().GetModelsAsync();

            // Compatible models are those that start with "gpt" or "oX" where X is a digit
            // By now this is the only way to filter models compatible with chat completions
            // Exclude models with EXCLUDED_WORDS in their name (e.g. "gpt-4o-realtime" or "gpt-image-1")
            var compatibleModels = models.Value.Where
            (
                m => !EXCLUDED_WORDS.Any(excludedWord => m.Id.Contains(excludedWord, StringComparison.InvariantCultureIgnoreCase))
                && (m.Id.StartsWith(GPT_MODEL_PREFIX, StringComparison.InvariantCultureIgnoreCase)
                    || (m.Id.Length > 1 && m.Id[0] == OX_MODEL_PREFIX[0] && char.IsDigit(m.Id[1])))
            ).OrderByDescending(m => m.CreatedAt);

            return string.IsNullOrEmpty(query)
                ? compatibleModels.Select(CreateAIModel)
                : compatibleModels.Where(m => m.Id.Contains(query)).Select(CreateAIModel);
        }

        /// <summary>
        /// Retrieves the OpenAI client instance, initializing it if necessary.
        /// </summary>
        /// <returns>An instance of <see cref="OpenAIClient"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the service is not properly initialized or fails to initialize.</exception>
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

        /// <summary>
        /// Creates an <see cref="AIModel"/> instance from an <see cref="OpenAIModel"/>.
        /// </summary>
        /// <param name="openAIModel">The OpenAI model to convert.</param>
        /// <returns>An instance of <see cref="AIModel"/> with metadata.</returns>
        private static AIModel CreateAIModel(OpenAIModel openAIModel)
        {
            string modelId = openAIModel.Id;
            string infoUrl = $"{OPENAI_MODELS_BASE_URL}{modelId}";

            if (modelId.Length > 10 && DateTime.TryParseExact(modelId[^10..], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
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