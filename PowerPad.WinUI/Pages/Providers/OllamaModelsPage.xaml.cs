using PowerPad.WinUI.ViewModels.AI;

namespace PowerPad.WinUI.Pages.Providers
{
    public partial class OllamaModelsPage : AIModelsPageBase
    {
        private OllamaModelsViewModel OllamaModelViewModel => (OllamaModelsViewModel)_modelsViewModel;

        public OllamaModelsPage()
            : base(new OllamaModelsViewModel())
        {
            this.InitializeComponent();
        }
    }
}