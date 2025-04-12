using PowerPad.WinUI.ViewModels.AI;
using System.Reflection.PortableExecutable;
using System;
using Microsoft.UI.Xaml;
using PowerPad.WinUI.Components;

namespace PowerPad.WinUI.Pages.Providers
{
    public partial class GitHubModelsPage : AIModelsPageBase, IModelProviderPage
    {
        public GitHubModelsPage()
            : base(new GitHubModelsViewModel())
        {
            this.InitializeComponent();
        }

        private void AIModelsRepeater_ModelInfoViewerVisibilityChanged(object _, Components.Controls.ModelInfoViewerVisibilityEventArgs eventArgs)
        {
            RowHeader.Height = eventArgs.IsVisible
                ? new(0, GridUnitType.Pixel)
                : new(1, GridUnitType.Auto);
        }

        public void CloseModelInfoViewer() => AvailableModelsRepeater.CloseModelInfoViewer();
    }
}