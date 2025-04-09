using PowerPad.WinUI.ViewModels.AI;

namespace PowerPad.WinUI.Pages.Providers
{
    public partial class HuggingFaceModelsPage : AIModelsPageBase
    {
        private HuggingFaceModelsViewModel HuggingFaceModelsViewModel => (HuggingFaceModelsViewModel)_modelsViewModel;

        public HuggingFaceModelsPage()
            : base(new HuggingFaceModelsViewModel())
        {
            this.InitializeComponent();
        }
    }
}