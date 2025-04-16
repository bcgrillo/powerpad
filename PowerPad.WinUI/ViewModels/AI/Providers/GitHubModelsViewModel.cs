using PowerPad.Core.Models.AI;
using PowerPad.Core.Services.AI;

namespace PowerPad.WinUI.ViewModels.AI.Providers
{
    public partial class GitHubModelsViewModel : AIModelsViewModelBase
    {
        public GitHubModelsViewModel()
            : base(App.Get<IAzureAIService>(), ModelProvider.GitHub)
        {
        }
    }
}