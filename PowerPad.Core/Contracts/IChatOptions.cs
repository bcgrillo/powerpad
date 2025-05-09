namespace PowerPad.Core.Contracts
{
    /// <summary>
    /// Defines the configuration options for chat behavior.
    /// </summary>
    internal interface IChatOptions
    {
        /// <summary>
        /// Gets or sets the temperature value, which controls the randomness of the chat responses.
        /// A higher value results in more random responses.
        /// </summary>
        float? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the Top-P value, which controls the cumulative probability for token sampling.
        /// A lower value results in more focused responses.
        /// </summary>
        float? TopP { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens allowed in the output response.
        /// </summary>
        int? MaxOutputTokens { get; set; }
    }
}