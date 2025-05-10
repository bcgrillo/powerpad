using PowerPad.Core.Contracts;

namespace PowerPad.Core.Models.AI
{
    /// <summary>
    /// Represents the parameters used to configure AI behavior in the system.
    /// Implements the <see cref="IChatOptions"/> interface to provide chat-specific options.
    /// </summary>
    public record AIParameters : IChatOptions
    {
        /// <summary>
        /// Gets or sets the system prompt, which serves as the initial instruction or context for the AI.
        /// </summary>
        public string? SystemPrompt { get; set; } = null;

        /// <inheritdoc />
        public float? Temperature { get; set; } = null;

        /// <inheritdoc />
        public float? TopP { get; set; } = null;

        /// <inheritdoc />
        public int? MaxOutputTokens { get; set; } = null;

        /// <summary>
        /// Gets or sets the maximum length of a conversation in terms of the number of messages.
        /// </summary>
        public int? MaxConversationLength { get; set; } = null;
    }
}