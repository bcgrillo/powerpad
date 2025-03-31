using PowerPad.WinUI.ViewModels.AI;

namespace PowerPad.WinUI.Pages.Providers
{
    public sealed partial class OpenAIModelsPage : AIModelsPageBase
    {
        public OpenAIModelsPage()
            : base(new OpenAIModelsViewModel())
        {
            this.InitializeComponent();
        }
    }
}