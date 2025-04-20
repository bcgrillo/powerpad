using Microsoft.Extensions.AI;
using PowerPad.Core.Models.AI;

namespace PowerPad.Core.Contracts
{
    public interface IAIService
    {
        void Initialize(AIServiceConfig? config);
        Task<TestConnectionResult> TestConection();
        IChatClient ChatClient(AIModel model, out IEnumerable<string> notAllowedParamers);
        Task<IEnumerable<AIModel>> SearchModels(ModelProvider modelProvider, string? query);
    }

    public readonly record struct TestConnectionResult(ServiceStatus Status, string? ErrorMessage = null);
}