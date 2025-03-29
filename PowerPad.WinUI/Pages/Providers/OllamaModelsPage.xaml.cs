using PowerPad.WinUI.ViewModels.AI;

namespace PowerPad.WinUI.Pages.Providers
{
    public sealed partial class OllamaModelsPage : AIModelsPageBase
    {
        public OllamaModelsPage()
        {
            this.InitializeComponent();

            _modelsViewModel = new OllamaModelsViewModel();
        }
    }
}