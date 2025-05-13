using Microsoft.UI.Xaml;
using PowerPad.WinUI.Components;
using PowerPad.WinUI.ViewModels.AI.Providers;

namespace PowerPad.WinUI.Pages.Providers
{
    /// <summary>
    /// Represents the page for managing OpenAI models.
    /// Inherits from <see cref="AIModelsPageBase"/>.
    /// </summary>
    public partial class OpenAIModelsPage : AIModelsPageBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAIModelsPage"/> class.
        /// </summary>
        public OpenAIModelsPage()
            : base(new OpenAIModelsViewModel())
        {
            this.InitializeComponent();
        }

        /// <inheritdoc />
        public override void CloseModelInfoViewer() => AvailableModelsRepeater.CloseModelInfoViewer();

        /// <summary>
        /// Handles the visibility change of the model info viewer in the AI models repeater.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="eventArgs">The event arguments containing visibility information.</param>
        private void AIModelsRepeater_ModelInfoViewerVisibilityChanged(object _, Components.Controls.ModelInfoViewerVisibilityEventArgs eventArgs)
        {
            RowHeader.Height = eventArgs.IsVisible
                ? new(0, GridUnitType.Pixel)
                : new(1, GridUnitType.Auto);
        }
    }
}