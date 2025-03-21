using Azure.AI.Inference;
using Microsoft.Extensions.AI;
using OllamaSharp.Models;
using PowerPad.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatRole = Microsoft.Extensions.AI.ChatRole;

namespace PowerPad.Core.Services
{
    public interface IAIService
    {
        void SetDefaultModel(AIModel defaultModel);
        void SetDefaultConfig(AIConfig? defaultConfig);
        Task<ChatResponse> GetResponse(string message, AIModel? model = null, AIConfig? config = null, CancellationToken cancellationToken = default);
        Task<ChatResponse> GetResponse(IList<ChatMessage> messages, AIModel? model = null, AIConfig? config = null, CancellationToken cancellationToken = default);
        IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponse(string message, AIModel? model = null, AIConfig? config = null, CancellationToken cancellationToken = default);
        IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponse(IList<ChatMessage> messages, AIModel? model = null, AIConfig? config = null, CancellationToken cancellationToken = default);
    }

    public class AIService : IAIService
    {
        private AIModel _defaultModel;
        private AIConfig? _defaultConfig;

        private IOllamaService _ollamaService;

        public AIService(AIModel defaultModel, IOllamaService ollamaService)
        {
            _defaultModel = defaultModel;
            _ollamaService = ollamaService;
        }

        public void SetDefaultModel(AIModel defaultModel)
        {
            _defaultModel = defaultModel;
        }

        public void SetDefaultConfig(AIConfig? defaultConfig)
        {
            _defaultConfig = defaultConfig;
        }

        private IChatClient ChatClient(AIModel model)
        {
            switch (model.ModelProvider)
            {
                case ModelProvider.Ollama:
                    return _ollamaService.ChatClient(model);
                default:
                    throw new NotImplementedException($"Client for provider {model.ModelProvider} not implemented.");
            }
        }

        public Task<ChatResponse> GetResponse(string message, AIModel? model = null, AIConfig? config = null, CancellationToken cancellationToken = default)
        {
            return GetResponse([new ChatMessage(ChatRole.User, message)], model, config, cancellationToken);
        }


        public Task<ChatResponse> GetResponse(IList<ChatMessage> messages, AIModel? model = null, AIConfig? config = null, CancellationToken cancellationToken = default)
        {
            var (chatClient, chatOption, messagesAux) = PrepareChatParameters(messages, model, config);

            return chatClient.GetResponseAsync(messagesAux, chatOption, cancellationToken);
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponse(string message, AIModel? model = null, AIConfig? config = null, CancellationToken cancellationToken = default)
        {
            return GetStreamingResponse([new ChatMessage(ChatRole.User, message)], model, config, cancellationToken);
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponse(IList<ChatMessage> messages, AIModel? model = null, AIConfig? config = null, CancellationToken cancellationToken = default)
        {
            var (chatClient, chatOption, messagesAux) = PrepareChatParameters(messages, model, config);

            return chatClient.GetStreamingResponseAsync(messagesAux, chatOption, cancellationToken);
        }

        private (IChatClient chatClient, ChatOptions? chatOption, IList<ChatMessage> chatMessages) PrepareChatParameters(IList<ChatMessage> chatMessages, AIModel? model, AIConfig? config)
        {
            model ??= _defaultModel;
            config ??= _defaultConfig;

            var chatClient = ChatClient(model);

            ChatOptions? chatOption = null;
            var messagesAux = chatMessages;

            if (config != null)
            {
                chatOption = new ChatOptions
                {
                    Temperature = config.Temperature,
                    MaxOutputTokens = config.MaxOutputTokens
                };

                if (!string.IsNullOrEmpty(config.SystemPrompt))
                {
                    messagesAux = [new ChatMessage(ChatRole.System, config.SystemPrompt), .. chatMessages];
                }
            }

            return (chatClient, chatOption, messagesAux);
        }
    }
}
