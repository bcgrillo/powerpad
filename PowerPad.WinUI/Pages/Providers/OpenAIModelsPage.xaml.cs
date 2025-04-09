using PowerPad.WinUI.ViewModels.AI;

namespace PowerPad.WinUI.Pages.Providers
{
    public partial class OpenAIModelsPage : AIModelsPageBase
    {
        public OpenAIModelsPage()
            : base(new OpenAIModelsViewModel())
        {
            this.InitializeComponent();
        }
    }
}