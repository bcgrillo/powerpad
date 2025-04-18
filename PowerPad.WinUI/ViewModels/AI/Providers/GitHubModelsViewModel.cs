using PowerPad.Core.Models.AI;

namespace PowerPad.WinUI.ViewModels.AI.Providers
{
    public partial class GitHubModelsViewModel : AIModelsViewModelBase
    {
        public GitHubModelsViewModel()
            : base(ModelProvider.GitHub)
        {
        }
    }
}