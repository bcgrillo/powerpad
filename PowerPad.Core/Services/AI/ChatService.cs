using Microsoft.Extensions.AI;
using PowerPad.Core.Models.AI;
using ChatRole = Microsoft.Extensions.AI.ChatRole;

namespace PowerPad.Core.Services.AI
{
    public interface IChatService
    {
        void SetDefaultModel(AIModel? defaultModel);
        void SetDefaultParameters(AIParameters? defaultConfig);
        Task<ChatResponse> GetResponse(string message, AIModel? model = null, AIParameters? config = null, CancellationToken cancellationToken = default);
        Task<ChatResponse> GetResponse(IList<ChatMessage> messages, AIModel? model = null, AIParameters? config = null, CancellationToken cancellationToken = default);
        IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponse(string message, AIModel? model = null, AIParameters? config = null, CancellationToken cancellationToken = default);
        IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponse(IList<ChatMessage> messages, AIModel? model = null, AIParameters? config = null, CancellationToken cancellationToken = default);
    }

    public class ChatService(IOllamaService ollamaService, IAzureAIService azureAIService, IOpenAIService openAIService) : IChatService
    {
        private AIModel? _defaultModel;
        private AIParameters? _defaultParameters;

        private readonly IOllamaService _ollamaService = ollamaService;
        private readonly IAzureAIService _azureAIService = azureAIService;
        private readonly IOpenAIService _openAIService = openAIService;

        public void SetDefaultModel(AIModel? defaultModel)
        {
            _defaultModel = defaultModel;
        }

        public void SetDefaultParameters(AIParameters? defaultParameters)
        {
            _defaultParameters = defaultParameters;
        }

        private IChatClient ChatClient(AIModel model)
        {
            var chatClient = model.ModelProvider switch
            {
                ModelProvider.Ollama or ModelProvider.HuggingFace => _ollamaService.ChatClient(model),
                ModelProvider.GitHub => _azureAIService.ChatClient(model),
                ModelProvider.OpenAI => _openAIService.ChatClient(model),
                _ => throw new NotImplementedException($"Client for provider {model.ModelProvider} not implemented."),
            };

            return chatClient ?? throw new InvalidOperationException($"Client for provider {model.ModelProvider} not initialized.");
        }

        public Task<ChatResponse> GetResponse(string message, AIModel? model = null, AIParameters? config = null, CancellationToken cancellationToken = default)
        {
            return GetResponse([new(ChatRole.User, message)], model, config, cancellationToken);
        }


        public Task<ChatResponse> GetResponse(IList<ChatMessage> messages, AIModel? model = null, AIParameters? config = null, CancellationToken cancellationToken = default)
        {
            var (chatClient, chatOption, messagesAux) = PrepareChatParameters(messages, model, config);

            return chatClient.GetResponseAsync(messagesAux, chatOption, cancellationToken);
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponse(string message, AIModel? model = null, AIParameters? config = null, CancellationToken cancellationToken = default)
        {
            return GetStreamingResponse([new(ChatRole.User, message)], model, config, cancellationToken);
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponse(IList<ChatMessage> messages, AIModel? model = null, AIParameters? config = null, CancellationToken cancellationToken = default)
        {
            var (chatClient, chatOption, messagesAux) = PrepareChatParameters(messages, model, config);

            return chatClient.GetStreamingResponseAsync(messagesAux, chatOption, cancellationToken);
        }

        private (IChatClient chatClient, ChatOptions? chatOption, IList<ChatMessage> chatMessages) PrepareChatParameters(IList<ChatMessage> chatMessages, AIModel? model, AIParameters? parameters)
        {
            model ??= _defaultModel;
            parameters ??= _defaultParameters;

            if (model is null) throw new InvalidOperationException("The model is missing and there is no default model set.");

            var chatClient = ChatClient(model);

            ChatOptions? chatOption = null;
            var messagesAux = chatMessages;

            if (parameters is not null)
            {
                chatOption = new()
                {
                    Temperature = parameters.Temperature,
                    TopP = parameters.TopP,
                    MaxOutputTokens = parameters.MaxOutputTokens,
                };

                if (!string.IsNullOrEmpty(parameters.SystemPrompt))
                {
                    messagesAux = 
                    [
                        new(ChatRole.System, parameters.SystemPrompt), 
                        ..
                        parameters.MaxConversationLength.HasValue
                        ? messagesAux.TakeLast(parameters.MaxConversationLength.Value)
                        : messagesAux
                    ];
                }
            }

            return (chatClient, chatOption, messagesAux);
        }
    }
}
