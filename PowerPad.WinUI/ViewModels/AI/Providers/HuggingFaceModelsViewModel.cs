using PowerPad.Core.Models.AI;
using PowerPad.Core.Services.AI;

namespace PowerPad.WinUI.ViewModels.AI.Providers
{
    public partial class HuggingFaceModelsViewModel : OllamaModelsViewModel
    {
        public HuggingFaceModelsViewModel()
            : base(App.Get<IOllamaService>(), ModelProvider.HuggingFace)
        {
        }
    }
}