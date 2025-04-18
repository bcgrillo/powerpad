using Microsoft.UI.Xaml;
using PowerPad.WinUI.Components;
using PowerPad.WinUI.ViewModels.AI.Providers;

namespace PowerPad.WinUI.Pages.Providers
{
    public partial class HuggingFaceModelsPage : AIModelsPageBase, IModelProviderPage
    {
        private HuggingFaceModelsViewModel _huggingFaceModelsViewModel => (HuggingFaceModelsViewModel)_modelsViewModel;

        public HuggingFaceModelsPage()
            : base(new HuggingFaceModelsViewModel())
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