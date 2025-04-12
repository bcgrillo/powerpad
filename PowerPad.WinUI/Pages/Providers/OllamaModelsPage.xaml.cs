using Microsoft.UI.Xaml;
using PowerPad.WinUI.Components;
using PowerPad.WinUI.ViewModels.AI;

namespace PowerPad.WinUI.Pages.Providers
{
    public partial class OllamaModelsPage : AIModelsPageBase, IModelProviderPage
    {
        private OllamaModelsViewModel OllamaModelsViewModel => (OllamaModelsViewModel)_modelsViewModel;

        public OllamaModelsPage()
            : base(new OllamaModelsViewModel())
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