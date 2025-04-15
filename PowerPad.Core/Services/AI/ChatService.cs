using Microsoft.Extensions.AI;
using OllamaSharp.Models.Chat;
using OllamaSharp.Models;
using PowerPad.Core.Models.AI;
using System.Text;
using ChatRole = Microsoft.Extensions.AI.ChatRole;
using System.Reflection;

namespace PowerPad.Core.Services.AI
{
    public interface IChatService
    {
        void SetDefaultModel(AIModel? defaultModel);
        void SetDefaultParameters(AIParameters? defaultConfig);
        IAsyncEnumerable<ChatResponseUpdate> GetChatResponse(IList<ChatMessage> messages, AIModel? model = null, AIParameters? config = null, CancellationToken cancellationToken = default);
        Task GetAgentResponse(string input, StringBuilder output, Agent selectedAgent, string? promptParameterValue, string? agentPrompt, CancellationToken cancellationToken = default);
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

        public IAsyncEnumerable<ChatResponseUpdate> GetChatResponse(IList<ChatMessage> messages, AIModel? model = null, AIParameters? parameters = null, CancellationToken cancellationToken = default)
        {
            var (chatClient, chatOptions, chatMessages) = PrepareChatParameters(messages, model, parameters);

            return chatClient.GetStreamingResponseAsync(chatMessages, chatOptions, cancellationToken);
        }

        public async Task GetAgentResponse(string input, StringBuilder output, Agent selectedAgent, string? promptParameterValue, string? agentPrompt, CancellationToken cancellationToken = default)
        {
            var (chatClient, chatOption, chatMessages) = PrepareAgentParameters(input, selectedAgent, promptParameterValue, agentPrompt);

            output.Append(await chatClient.GetResponseAsync(chatMessages, chatOption, cancellationToken));
        }

        private (IChatClient chatClient, ChatOptions? chatOptions, IList<ChatMessage> chatMessages) PrepareChatParameters(IList<ChatMessage> messages, AIModel? model, AIParameters? parameters)
        {
            model ??= _defaultModel;
            parameters ??= _defaultParameters;

            if (model is null) throw new InvalidOperationException("The model is missing and there is no default model set.");

            var chatClient = ChatClient(model);

            ChatOptions? chatOption = null;
            List<ChatMessage> chatMessages = [];

            if (parameters is not null)
            {
                chatOption = new()
                {
                    Temperature = parameters.Temperature,
                    TopP = parameters.TopP,
                    MaxOutputTokens = parameters.MaxOutputTokens,
                };

                if (!string.IsNullOrEmpty(parameters.SystemPrompt))
                    chatMessages.Add(new(ChatRole.System, parameters.SystemPrompt));

                if (parameters.MaxConversationLength.HasValue)
                    chatMessages.AddRange(messages.TakeLast(parameters.MaxConversationLength.Value));
                else
                    chatMessages.AddRange(messages);
            }

            return (chatClient, chatOption, chatMessages);
        }

        private (IChatClient chatClient, ChatOptions? chatOptions, IList<ChatMessage> chatMessages) PrepareAgentParameters(string input, Agent agent, string? promptParameterValue, string? agentPrompt)
        {
            var model = agent.AIModel;
            model ??= _defaultModel;

            if (model is null) throw new InvalidOperationException("The model is missing and there is no default model set.");

            var chatClient = ChatClient(model);

            var chatMessages = new List<ChatMessage>
            {
                new
                (
                    ChatRole.System,
                    agent.Prompt
                        + (!string.IsNullOrEmpty(agent.PromptParameterName) ? $"\n{agent.PromptParameterName}: {promptParameterValue}" : string.Empty)
                        + (!string.IsNullOrEmpty(agentPrompt) ? $"\n{agentPrompt}" : string.Empty)
                ),
                new(ChatRole.User, input)
            };

            var chatOption = new ChatOptions
            {
                Temperature = agent.Temperature,
                TopP = agent.TopP,
                MaxOutputTokens = agent.MaxOutputTokens,
            };

            return (chatClient, chatOption, chatMessages);
        }
    }
}
