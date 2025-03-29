using PowerPad.Core.Models.AI;
using PowerPad.Core.Services.AI;

namespace PowerPad.WinUI.ViewModels.AI
{
    public partial class HuggingFaceModelsViewModel : AIModelsViewModelBase
    {
        public HuggingFaceModelsViewModel()
            : base(App.Get<IOllamaService>(), ModelProvider.HuggingFace)
        {
        }
    }
}