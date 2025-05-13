namespace PowerPad.Core.Models.AI
{
    /// <summary>
    /// Represents the different providers for AI models supported by the application.
    /// </summary>
    public enum ModelProvider
    {
        /// <summary>Ollama Library Provider</summary>
        Ollama,
        /// <summary>HuggingFace Provider</summary>
        HuggingFace,
        /// <summary>GitHub Models Provider</summary>
        GitHub,
        /// <summary>OpenAI Provider</summary>
        OpenAI,
    }
}