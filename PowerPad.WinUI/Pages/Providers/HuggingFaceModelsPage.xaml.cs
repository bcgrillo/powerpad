using PowerPad.WinUI.ViewModels.AI;

namespace PowerPad.WinUI.Pages.Providers
{
    public sealed partial class HuggingFaceModelsPage : AIModelsPageBase
    {
        public HuggingFaceModelsPage()
        {
            this.InitializeComponent();

            _modelsViewModel = new HuggingFaceModelsViewModel();
        }
    }
}