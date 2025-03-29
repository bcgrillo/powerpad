using Microsoft.Extensions.AI;
using PowerPad.Core.Models.AI;

namespace PowerPad.Core.Contracts
{
    public interface IAIService
    {
        Task<IEnumerable<AIModel>> GetAvailableModels();
        IChatClient? ChatClient(AIModel model);
    }
}
