using Microsoft.Extensions.AI;
using PowerPad.Core.Models.AI;

namespace PowerPad.Core.Contracts
{
    /// <summary>
    /// Defines the contract for an AI service, providing methods for initialization, testing connections,
    /// managing chat clients, and searching AI models.
    /// </summary>
    public interface IAIService
    {
        /// <summary>
        /// Initializes the AI service with the specified configuration.
        /// </summary>
        /// <param name="config">The configuration settings for the AI service. Can be null.</param>
        void Initialize(AIServiceConfig? config);

        /// <summary>
        /// Tests the connection to the AI service.
        /// </summary>
        /// <returns>A <see cref="TestConnectionResult"/> indicating the status of the connection test.</returns>
        Task<TestConnectionResult> TestConnection();

        /// <summary>
        /// Creates a chat client for the specified AI model.
        /// </summary>
        /// <param name="model">The AI model to use for the chat client.</param>
        /// <param name="notAllowedParameters">Outputs a list of parameters that are not allowed for the specified model.</param>
        /// <returns>An instance of <see cref="IChatClient"/> for interacting with the AI model.</returns>
        IChatClient ChatClient(AIModel model, out IEnumerable<string>? notAllowedParameters);

        /// <summary>
        /// Searches for AI models based on the specified provider and query.
        /// </summary>
        /// <param name="modelProvider">The provider of the AI models (e.g., OpenAI, HuggingFace).</param>
        /// <param name="query">An optional query string to filter the models.</param>
        /// <returns>A collection of <see cref="AIModel"/> that match the search criteria.</returns>
        Task<IEnumerable<AIModel>> SearchModels(ModelProvider modelProvider, string? query);
    }

    /// <summary>
    /// Represents the result of testing the connection to the AI service.
    /// </summary>
    /// <param name="Status">The status of the service connection.</param>
    /// <param name="ErrorMessage">An optional error message if the connection test failed.</param>
    public readonly record struct TestConnectionResult(ServiceStatus Status, string? ErrorMessage = null);
}