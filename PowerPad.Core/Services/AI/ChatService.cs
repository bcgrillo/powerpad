using Microsoft.Extensions.AI;
using PowerPad.Core.Models.AI;
using System.Text;
using ChatRole = Microsoft.Extensions.AI.ChatRole;
using PowerPad.Core.Contracts;

namespace PowerPad.Core.Services.AI
{
    public interface IChatService
    {
        void SetDefaultModel(AIModel? defaultModel);
        void SetDefaultParameters(AIParameters? defaultConfig);
        IAsyncEnumerable<ChatResponseUpdate> GetChatResponse(IList<ChatMessage> messages, AIModel? model = null, AIParameters? config = null, CancellationToken cancellationToken = default);
        Task GetAgentResponse(string input, StringBuilder output, Agent selectedAgent, string? promptParameterValue, string? agentPrompt, CancellationToken cancellationToken = default);
    }

    public class ChatService(IReadOnlyDictionary<ModelProvider, IAIService> aiServices) : IChatService
    {
        private const string THINK_START_TAG = "<think>";
        private const string THINK_END_TAG = "</think>";

        private AIModel? _defaultModel;
        private AIParameters? _defaultParameters;
        private IEnumerable<string>? _notAllowedParameters;

        private readonly IReadOnlyDictionary<ModelProvider, IAIService> _aiServices = aiServices;

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
            if (_aiServices.TryGetValue(model.ModelProvider, out var aiService))
            {
                return aiService.ChatClient(model, out _notAllowedParameters);
            }
            else
            {
                throw new NotImplementedException($"Client for provider {model.ModelProvider} not implemented.");
            }
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetChatResponse(IList<ChatMessage> messages, AIModel? model = null, AIParameters? parameters = null, CancellationToken cancellationToken = default)
        {
            var (chatClient, chatOptions, chatMessages) = PrepareChatParameters(messages, model, parameters);
            
            return chatClient.GetStreamingResponseAsync(chatMessages, chatOptions, cancellationToken);
        }

        public async Task GetAgentResponse(string input, StringBuilder output, Agent selectedAgent, string? promptParameterValue, string? agentPrompt, CancellationToken cancellationToken = default)
        {
            var (chatClient, chatOption, chatMessages) = PrepareAgentParameters(input, selectedAgent, promptParameterValue, agentPrompt);

            var response = (await chatClient.GetResponseAsync(chatMessages, chatOption, cancellationToken)).Text;

            // Remove content between <think> and </think>, or clear the response if </think> is missing
            var startIndex = response.IndexOf(THINK_START_TAG);
            if (startIndex != -1)
            {
                var endIndex = response.IndexOf(THINK_END_TAG, startIndex);
                if (endIndex != -1) response = response.Remove(startIndex, endIndex + THINK_END_TAG.Length - startIndex);
                else response = string.Empty;
            }

            output.Append(response);
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
                chatOption = PrepareChatOptions(parameters);

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

            var chatOption = PrepareChatOptions(agent);

            return (chatClient, chatOption, chatMessages);
        }

        private ChatOptions PrepareChatOptions(IChatOptions chatoptions)
        {
            ChatOptions chatOption;

            if (_notAllowedParameters is not null)
            {
                chatOption = new()
                {
                    Temperature = _notAllowedParameters.Contains(nameof(IChatOptions.Temperature)) ? null : chatoptions.Temperature,
                    TopP = _notAllowedParameters.Contains(nameof(IChatOptions.TopP)) ? null : chatoptions.TopP,
                    MaxOutputTokens = _notAllowedParameters.Contains(nameof(IChatOptions.MaxOutputTokens)) ? null : chatoptions.MaxOutputTokens,
                };
            }
            else
            {
                chatOption = new()
                {
                    Temperature = chatoptions.Temperature,
                    TopP = chatoptions.TopP,
                    MaxOutputTokens = chatoptions.MaxOutputTokens,
                };
            }

            return chatOption;
        }
    }
}
