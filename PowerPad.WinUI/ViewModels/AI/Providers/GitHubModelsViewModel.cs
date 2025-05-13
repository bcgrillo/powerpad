using PowerPad.Core.Models.AI;

namespace PowerPad.WinUI.ViewModels.AI.Providers
{
    /// <summary>
    /// ViewModel for managing GitHub AI models.
    /// Inherits from <see cref="AIModelsViewModelBase"/> and provides functionality specific to GitHub models.
    /// </summary>
    public partial class GitHubModelsViewModel : AIModelsViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubModelsViewModel"/> class.
        /// Sets the model provider to <see cref="ModelProvider.GitHub"/>.
        /// </summary>
        public GitHubModelsViewModel()
            : base(ModelProvider.GitHub)
        {
        }
    }
}