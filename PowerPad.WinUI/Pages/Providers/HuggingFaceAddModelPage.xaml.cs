using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.Components;
using PowerPad.WinUI.ViewModels.AI.Providers;

namespace PowerPad.WinUI.Pages.Providers
{
    /// <summary>
    /// Represents the page for adding Hugging Face models.
    /// Inherits from <see cref="AIAddModelPageBase"/>.
    /// </summary>
    public partial class HuggingFaceAddModelPage : AIAddModelPageBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HuggingFaceAddModelPage"/> class.
        /// </summary>
        public HuggingFaceAddModelPage()
            : base(new HuggingFaceModelsViewModel())
        {
            this.InitializeComponent();
        }

        /// <inheritdoc />
        public override void Search()
        {
            RowHeader.Height = new(1, GridUnitType.Auto);
            base.Search();
        }

        /// <inheritdoc />
        public override void CloseModelInfoViewer() => SearchModelsResultRepeater.CloseModelInfoViewer();

        /// <inheritdoc />
        protected override TextBox GetSearchTextBox() => SearchTextBox;

        /// <summary>
        /// Handles the visibility change of the model info viewer.
        /// Adjusts the header row height based on the visibility state.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="eventArgs">The event arguments containing visibility state.</param>
        private void SearchModelsResultRepeater_ModelInfoViewerVisibilityChanged(object _, Components.Controls.ModelInfoViewerVisibilityEventArgs eventArgs)
        {
            RowHeader.Height = eventArgs.IsVisible
                ? new(0, GridUnitType.Pixel)
                : new(1, GridUnitType.Auto);
        }
    }
}