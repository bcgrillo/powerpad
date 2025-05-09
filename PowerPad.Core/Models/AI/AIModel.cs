namespace PowerPad.Core.Models.AI
{
    /// <summary>
    /// Represents an AI model with its associated metadata.
    /// </summary>
    /// <param name="Name">The unique name of the AI model.</param>
    /// <param name="ModelProvider">The provider of the AI model (e.g., OpenAI, HuggingFace).</param>
    /// <param name="InfoUrl">An optional URL with more information about the model.</param>
    /// <param name="Size">The optional size of the model in bytes.</param>
    /// <param name="DisplayName">An optional display name for the model.</param>
    public record AIModel(string Name, ModelProvider ModelProvider, string? InfoUrl, long? Size = null, string? DisplayName = null)
    {
        /// <summary>
        /// Gets the unique name of the AI model.
        /// </summary>
        public string Name { get; init; } = Name;

        /// <summary>
        /// Gets the provider of the AI model.
        /// </summary>
        public ModelProvider ModelProvider { get; init; } = ModelProvider;

        /// <summary>
        /// Gets an optional URL with more information about the model.
        /// </summary>
        public string? InfoUrl { get; init; } = InfoUrl;

        /// <summary>
        /// Gets or sets the optional size of the model in bytes.
        /// </summary>
        public long? Size { get; set; } = Size;

        /// <summary>
        /// Gets or sets an optional display name for the model.
        /// </summary>
        public string? DisplayName { get; set; } = DisplayName;
    }
}