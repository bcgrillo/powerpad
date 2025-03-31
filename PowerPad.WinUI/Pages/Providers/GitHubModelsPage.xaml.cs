using PowerPad.WinUI.ViewModels.AI;

namespace PowerPad.WinUI.Pages.Providers
{
    public sealed partial class GitHubModelsPage : AIModelsPageBase
    {
        public GitHubModelsPage()
            : base(new GitHubModelsViewModel())
        {
            this.InitializeComponent();
        }
    }
}