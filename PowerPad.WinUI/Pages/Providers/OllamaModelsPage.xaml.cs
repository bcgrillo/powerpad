using Microsoft.UI.Xaml;
using PowerPad.WinUI.Components;
using PowerPad.WinUI.ViewModels.AI.Providers;

namespace PowerPad.WinUI.Pages.Providers
{
    /// <summary>
    /// Represents the page for managing Ollama AI models.
    /// Inherits from <see cref="AIModelsPageBase"/>.
    /// </summary>
    public partial class OllamaModelsPage : AIModelsPageBase
    {
        /// <summary>
        /// Gets the view model for Ollama models.
        /// </summary>
        private OllamaModelsViewModel OllamaModelsViewModel => (OllamaModelsViewModel)_modelsViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="OllamaModelsPage"/> class.
        /// </summary>
        public OllamaModelsPage()
            : base(new OllamaModelsViewModel())
        {
            this.InitializeComponent();
        }

        /// <inheritdoc />
        public override void CloseModelInfoViewer() => AvailableModelsRepeater.CloseModelInfoViewer();

        /// <summary>
        /// Handles the visibility change of the model info viewer.
        /// Adjusts the height of the row header based on the visibility state.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="eventArgs">The event arguments containing visibility state.</param>
        private void AIModelsRepeater_ModelInfoViewerVisibilityChanged(object _, Components.Controls.ModelInfoViewerVisibilityEventArgs eventArgs)
        {
            RowHeader.Height = eventArgs.IsVisible
                ? new(0, GridUnitType.Pixel)
                : new(1, GridUnitType.Auto);
        }
    }
}