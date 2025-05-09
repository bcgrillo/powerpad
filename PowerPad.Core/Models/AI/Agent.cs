using PowerPad.Core.Contracts;

namespace PowerPad.Core.Models.AI
{
    /// <summary>
    /// Represents an AI Agent with specific parameters and configurations.
    /// </summary>
    public record Agent() : AgentParameters
    {
        /// <summary>
        /// Gets or sets the name of the agent.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the prompt used by the agent.
        /// </summary>
        public required string Prompt { get; set; }

        /// <summary>
        /// Gets or sets the name of the parameter used in the prompt, if any.
        /// </summary>
        public string? PromptParameterName { get; set; } = null;

        /// <summary>
        /// Gets or sets the description of the parameter used in the prompt, if any.
        /// </summary>
        public string? PromptParameterDescription { get; set; } = null;

        /// <summary>
        /// Gets or sets the AI model associated with the agent.
        /// </summary>
        public AIModel? AIModel { get; set; }

        /// <summary>
        /// Retrieves the chat options for the agent, using default parameters if necessary.
        /// </summary>
        /// <param name="defaultParameters">The default parameters to use if specific values are not set.</param>
        /// <returns>An instance of <see cref="IChatOptions"/> with the configured parameters.</returns>
        internal IChatOptions GetParameters(AIParameters? defaultParameters)
        {
            return new AgentParameters
            {
                Temperature = Temperature ?? defaultParameters?.Temperature,
                TopP = TopP ?? defaultParameters?.TopP,
                MaxOutputTokens = MaxOutputTokens ?? defaultParameters?.MaxOutputTokens
            };
        }
    }

    /// <summary>
    /// Represents the parameters for configuring an AI agent's behavior.
    /// </summary>
    public record AgentParameters : IChatOptions
    {
        /// <inheritdoc/>
        public float? Temperature { get; set; } = null;

        /// <inheritdoc/>
        public float? TopP { get; set; } = null;

        /// <inheritdoc/>
        public int? MaxOutputTokens { get; set; } = null;
    }
}