using Microsoft.Extensions.AI;
using PowerPad.Core.Models.AI;
using System.Text;
using ChatRole = Microsoft.Extensions.AI.ChatRole;
using PowerPad.Core.Contracts;

namespace PowerPad.Core.Services.AI
{
    /// <summary>
    /// Defines the contract for a chat service, providing methods to interact with AI models and agents.
    /// </summary>
    public interface IChatService
    {

        /// <summary>
        /// Sets the default AI model to be used by the chat service.
        /// </summary>
        /// <param name="defaultModel">The AI model to set as the default. Can be null.</param>
        void SetDefaultModel(AIModel? defaultModel);


        /// <summary>
        /// Sets the default parameters for the chat service.
        /// </summary>
        /// <param name="defaultParameters">The parameters to set as default. Can be null.</param>
        void SetDefaultParameters(AIParameters? defaultParameters);

        /// <summary>
        /// Retrieves a streaming chat response based on the provided messages and optional model and parameters.
        /// </summary>
        /// <param name="messages">The list of chat messages to process.</param>
        /// <param name="model">The AI model to use. If null, the default model is used.</param>
        /// <param name="parameters">The parameters for the chat. If null, the default parameters are used.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>An asynchronous stream of chat response updates.</returns>
        IAsyncEnumerable<ChatResponseUpdate> GetChatResponse(IList<ChatMessage> messages, AIModel? model = null, AIParameters? parameters = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a streaming response from an AI agent based on the provided messages.
        /// </summary>
        /// <param name="messages">The list of chat messages to process.</param>
        /// <param name="agent">The AI agent to interact with.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>An asynchronous stream of chat response updates.</returns>
        IAsyncEnumerable<ChatResponseUpdate> GetAgentResponse(IList<ChatMessage> messages, Agent agent, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a single response from an AI agent based on the provided input and appends it to the output.
        /// </summary>
        /// <param name="input">The user input to process.</param>
        /// <param name="output">The output StringBuilder to append the response to.</param>
        /// <param name="selectedAgent">The AI agent to interact with.</param>
        /// <param name="promptParameterValue">An optional parameter value to include in the agent's prompt.</param>
        /// <param name="agentPrompt">An optional additional prompt for the agent.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task GetAgentSingleResponse(string input, StringBuilder output, Agent selectedAgent, string? promptParameterValue, string? agentPrompt, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Provides an implementation of the <see cref="IChatService"/> interface, enabling interaction with AI models and agents.
    /// </summary>
    public class ChatService(IReadOnlyDictionary<ModelProvider, IAIService> aiServices) : IChatService
    {
        private static readonly string[] THINK_START_TAG = ["<think>", "<thought>"];
        private static readonly string[] THINK_END_TAG = ["</think>", "</thought>"];

        private AIModel? _defaultModel;
        private AIParameters? _defaultParameters;

        private readonly IReadOnlyDictionary<ModelProvider, IAIService> _aiServices = aiServices;

        /// <inheritdoc />
        public void SetDefaultModel(AIModel? defaultModel)
        {
            _defaultModel = defaultModel;
        }

        /// <inheritdoc />
        public void SetDefaultParameters(AIParameters? defaultParameters)
        {
            _defaultParameters = defaultParameters;
        }

        /// <inheritdoc />
        public IAsyncEnumerable<ChatResponseUpdate> GetChatResponse(IList<ChatMessage> messages, AIModel? model = null, AIParameters? parameters = null, CancellationToken cancellationToken = default)
        {
            var (chatClient, chatOptions, chatMessages) = PrepareChatParameters(messages, model, parameters);

            return chatClient.GetStreamingResponseAsync(chatMessages, chatOptions, cancellationToken);
        }

        /// <inheritdoc />
        public IAsyncEnumerable<ChatResponseUpdate> GetAgentResponse(IList<ChatMessage> messages, Agent agent, CancellationToken cancellationToken = default)
        {
            var (chatClient, chatOptions, chatMessages) = PrepareAgentParameters(messages, agent);

            return chatClient.GetStreamingResponseAsync(chatMessages, chatOptions, cancellationToken);
        }

        /// <inheritdoc />
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

        /// <summary>
        /// Retrieves a chat client for the specified AI model.
        /// </summary>
        /// <param name="model">The AI model to use.</param>
        /// <param name="notAllowedParameters">Outputs a list of parameters that are not allowed for the specified model.</param>
        /// <returns>An instance of <see cref="IChatClient"/> for interacting with the AI model.</returns>
        /// <exception cref="NotImplementedException">Thrown if the client for the specified provider is not implemented.</exception>
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

        /// <summary>
        /// Prepares the parameters required for a chat interaction.
        /// </summary>
        /// <param name="messages">The list of chat messages to process.</param>
        /// <param name="model">The AI model to use. If null, the default model is used.</param>
        /// <param name="parameters">The configuration parameters for the chat. If null, the default parameters are used.</param>
        /// <returns>A tuple containing the chat client, chat options, and chat messages.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no model is provided or set as default.</exception>
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

        /// <summary>
        /// Prepares the parameters required for an agent interaction.
        /// </summary>
        /// <param name="messages">The list of chat messages to process.</param>
        /// <param name="agent">The AI agent to interact with.</param>
        /// <returns>A tuple containing the chat client, chat options, and chat messages.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no model is provided or set as default.</exception>
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

        /// <summary>
        /// Prepares the parameters required for an agent interaction with a single input.
        /// </summary>
        /// <param name="input">The user input to process.</param>
        /// <param name="agent">The AI agent to interact with.</param>
        /// <param name="promptParameterValue">An optional parameter value to include in the agent's prompt.</param>
        /// <param name="agentPrompt">An optional additional prompt for the agent.</param>
        /// <returns>A tuple containing the chat client, chat options, and chat messages.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no model is provided or set as default.</exception>
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

        /// <summary>
        /// Prepares the chat options based on the provided configuration and restricted parameters.
        /// </summary>
        /// <param name="chatoptions">The chat options to configure.</param>
        /// <param name="notAllowedParameters">A list of parameters that are not allowed for the specified model.</param>
        /// <returns>An instance of <see cref="ChatOptions"/> with the configured parameters.</returns>
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