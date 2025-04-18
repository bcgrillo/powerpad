using Microsoft.UI.Xaml;
using PowerPad.WinUI.Components;
using PowerPad.WinUI.ViewModels.AI.Providers;

namespace PowerPad.WinUI.Pages.Providers
{
    public partial class OpenAIModelsPage : AIModelsPageBase, IModelProviderPage
    {
        public OpenAIModelsPage()
            : base(new OpenAIModelsViewModel())
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