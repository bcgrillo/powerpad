using Microsoft.Extensions.AI;
using PowerPad.Core.Models.AI;
using System.Text;
using ChatRole = Microsoft.Extensions.AI.ChatRole;
using PowerPad.Core.Contracts;
using OllamaSharp.Models.Chat;
using OllamaSharp.Models;

namespace PowerPad.Core.Services.AI
{
    public interface IChatService
    {
        void SetDefaultModel(AIModel? defaultModel);
        void SetDefaultParameters(AIParameters? defaultConfig);
        IAsyncEnumerable<ChatResponseUpdate> GetChatResponse(IList<ChatMessage> messages, AIModel? model = null, AIParameters? config = null, CancellationToken cancellationToken = default);
        IAsyncEnumerable<ChatResponseUpdate> GetAgentResponse(IList<ChatMessage> messages, Agent agent, CancellationToken cancellationToken = default);
        Task GetAgentSingleResponse(string input, StringBuilder output, Agent selectedAgent, string? promptParameterValue, string? agentPrompt, CancellationToken cancellationToken = default);
    }

    public class ChatService(IReadOnlyDictionary<ModelProvider, IAIService> aiServices) : IChatService
    {
        private static readonly string[] THINK_START_TAG = ["<think>", "<thought>"];
        private static readonly string[] THINK_END_TAG = ["</think>", "</thought>"];

        private AIModel? _defaultModel;
        private AIParameters? _defaultParameters;

        private readonly IReadOnlyDictionary<ModelProvider, IAIService> _aiServices = aiServices;

        public void SetDefaultModel(AIModel? defaultModel)
        {
            _defaultModel = defaultModel;
        }

        public void SetDefaultParameters(AIParameters? defaultParameters)
        {
            _defaultParameters = defaultParameters;
        }

        private IChatClient ChatClient(AIModel model, out IEnumerable<string>? notAllowedParameters)
        {
            if (_aiServices.TryGetValue(model.ModelProvider, out var aiService))
            {
                return aiService.ChatClient(model, out notAllowedParameters);
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

        public IAsyncEnumerable<ChatResponseUpdate> GetAgentResponse(IList<ChatMessage> messages, Agent agent, CancellationToken cancellationToken = default)
        {
            var (chatClient, chatOptions, chatMessages) = PrepareAgentParameters(messages, agent);

            return chatClient.GetStreamingResponseAsync(chatMessages, chatOptions, cancellationToken);
        }

        public async Task GetAgentSingleResponse(string input, StringBuilder output, Agent agent, string? promptParameterValue, string? agentPrompt, CancellationToken cancellationToken = default)
        {
            var (chatClient, chatOption, chatMessages) = PrepareAgentParameters(input, agent, promptParameterValue, agentPrompt);

            var response = (await chatClient.GetResponseAsync(chatMessages, chatOption, cancellationToken)).Text;

            // Remove content between think tags, or clear the response if close think tag is missing
            var thinkStartTag = THINK_START_TAG.FirstOrDefault(tag => response.Contains(tag));

            if (thinkStartTag is not null)
            {
                var startIndex = response.IndexOf(thinkStartTag);

                var thinkEndTag = THINK_END_TAG.FirstOrDefault(tag => response.Contains(tag));

                if (thinkEndTag is not null)
                {
                    var endIndex = response.IndexOf(thinkEndTag, startIndex);

                    response = response.Remove(startIndex, endIndex + thinkEndTag.Length - startIndex);
                }
                else response = string.Empty;
            }

            output.Append(response);
        }

        private (IChatClient chatClient, ChatOptions? chatOptions, IList<ChatMessage> chatMessages) PrepareChatParameters(IList<ChatMessage> messages, AIModel? model, AIParameters? parameters)
        {
            model ??= _defaultModel;
            parameters ??= _defaultParameters;

            if (model is null) throw new InvalidOperationException("The model is missing and there is no default model set.");

            var chatClient = ChatClient(model, out var notAllowedParameters);

            ChatOptions? chatOptions = null;
            List<ChatMessage> chatMessages = [];

            if (parameters is not null)
            {
                chatOptions = PrepareChatOptions(parameters, notAllowedParameters);

                if (!string.IsNullOrEmpty(parameters.SystemPrompt))
                    chatMessages.Add(new(ChatRole.System, parameters.SystemPrompt));

                if (parameters.MaxConversationLength.HasValue)
                    chatMessages.AddRange(messages.TakeLast(parameters.MaxConversationLength.Value));
                else
                    chatMessages.AddRange(messages);
            }
            else
            {
                chatMessages.AddRange(messages);
            }

            return (chatClient, chatOptions, chatMessages);
        }

        private (IChatClient chatClient, ChatOptions? chatOptions, IList<ChatMessage> chatMessages) PrepareAgentParameters(IList<ChatMessage> messages, Agent agent)
        {
            var model = (agent.AIModel ?? _defaultModel)
                ?? throw new InvalidOperationException("The model is missing and there is no default model set.");

            var chatClient = ChatClient(model, out var notAllowedParameters);

            List<ChatMessage> chatMessages = [];

            var chatOptions = PrepareChatOptions(agent.GetParameters(_defaultParameters), notAllowedParameters);

            chatMessages.Add(new(ChatRole.System, agent.Prompt));

            if (_defaultParameters?.MaxConversationLength.HasValue == true)
                chatMessages.AddRange(messages.TakeLast(_defaultParameters.MaxConversationLength.Value));
            else
                chatMessages.AddRange(messages);

            return (chatClient, chatOptions, chatMessages);
        }

        private (IChatClient chatClient, ChatOptions? chatOptions, IList<ChatMessage> chatMessages) PrepareAgentParameters(string input, Agent agent, string? promptParameterValue, string? agentPrompt)
        {
            var model = (agent.AIModel ?? _defaultModel)
                ?? throw new InvalidOperationException("The model is missing and there is no default model set.");
            
            var chatClient = ChatClient(model, out var notAllowedParameters);

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

            var chatOptions = PrepareChatOptions(agent.GetParameters(_defaultParameters), notAllowedParameters);

            return (chatClient, chatOptions, chatMessages);
        }

        private static ChatOptions PrepareChatOptions(IChatOptions chatoptions, IEnumerable<string>? notAllowedParameters)
        {
            ChatOptions chatOption;

            if (notAllowedParameters is not null)
            {
                chatOption = new()
                {
                    Temperature = notAllowedParameters.Contains(nameof(IChatOptions.Temperature)) ? null : chatoptions.Temperature,
                    TopP = notAllowedParameters.Contains(nameof(IChatOptions.TopP)) ? null : chatoptions.TopP,
                    MaxOutputTokens = notAllowedParameters.Contains(nameof(IChatOptions.MaxOutputTokens)) ? null : chatoptions.MaxOutputTokens,
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
