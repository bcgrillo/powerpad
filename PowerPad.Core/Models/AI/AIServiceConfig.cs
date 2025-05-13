namespace PowerPad.Core.Models.AI
{
    /// <summary>
    /// Represents the configuration settings for an AI service.
    /// </summary>
    public record AIServiceConfig
    {
        /// <summary>
        /// Gets or sets the base URL of the AI service.
        /// </summary>
        public string? BaseUrl { get; set; } = null;

        /// <summary>
        /// Gets or sets the API key for accessing the AI service.
        /// </summary>
        public string? Key { get; set; } = null;
    }
}