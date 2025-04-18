using PowerPad.Core.Models.AI;

namespace PowerPad.WinUI.ViewModels.AI.Providers
{
    public partial class HuggingFaceModelsViewModel : OllamaModelsViewModel
    {
        public HuggingFaceModelsViewModel()
            : base(ModelProvider.HuggingFace)
        {
        }
    }
}