using PowerPad.Core.Models.AI;
using PowerPad.Core.Services.AI;

namespace PowerPad.WinUI.ViewModels.AI
{
    public partial class OpenAIModelsViewModel : AIModelsViewModelBase
    {
        public OpenAIModelsViewModel()
            : base(App.Get<IOpenAIService>(), ModelProvider.OpenAI)
        {
        }
    }
}