using PowerPad.Core.Models.AI;

namespace PowerPad.WinUI.ViewModels.AI.Providers
{
    /// <summary>
    /// ViewModel for managing HuggingFace AI models.
    /// Inherits from <see cref="OllamaModelsViewModel"/> to provide functionality specific to HuggingFace models.
    /// </summary>
    public partial class HuggingFaceModelsViewModel : OllamaModelsViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HuggingFaceModelsViewModel"/> class.
        /// Sets the model provider to <see cref="ModelProvider.HuggingFace"/>.
        /// </summary>
        public HuggingFaceModelsViewModel()
            : base(ModelProvider.HuggingFace)
        {
        }
    }
}