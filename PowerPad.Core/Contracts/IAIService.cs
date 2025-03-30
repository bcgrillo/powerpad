using Microsoft.Extensions.AI;
using PowerPad.Core.Models.AI;

namespace PowerPad.Core.Contracts
{
    public interface IAIService
    {
        IChatClient? ChatClient(AIModel model);
        Task<IEnumerable<AIModel>> SearchModels(ModelProvider modelProvider, string? query);
    }
}
