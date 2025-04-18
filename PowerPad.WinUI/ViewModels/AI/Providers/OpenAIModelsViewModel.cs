using PowerPad.Core.Models.AI;

namespace PowerPad.WinUI.ViewModels.AI.Providers
{
    public partial class OpenAIModelsViewModel : AIModelsViewModelBase
    {
        public OpenAIModelsViewModel()
            : base(ModelProvider.OpenAI)
        {
        }
    }
}