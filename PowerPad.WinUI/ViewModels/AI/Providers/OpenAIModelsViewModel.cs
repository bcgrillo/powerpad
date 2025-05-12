using PowerPad.Core.Models.AI;

namespace PowerPad.WinUI.ViewModels.AI.Providers
{
    /// <summary>
    /// ViewModel for managing OpenAI models in the application.
    /// Inherits from <see cref="AIModelsViewModelBase"/> and provides specific functionality for OpenAI models.
    /// </summary>
    public partial class OpenAIModelsViewModel : AIModelsViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAIModelsViewModel"/> class.
        /// </summary>
        public OpenAIModelsViewModel()
            : base(ModelProvider.OpenAI)
        {
        }
    }
}