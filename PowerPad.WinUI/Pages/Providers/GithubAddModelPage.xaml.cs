using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.ViewModels.AI.Providers;

namespace PowerPad.WinUI.Pages.Providers
{
    /// <summary>
    /// Represents a page for adding GitHub models.
    /// Inherits from <see cref="AIAddModelPageBase"/>.
    /// </summary>
    public partial class GitHubAddModelPage : AIAddModelPageBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubAddModelPage"/> class.
        /// </summary>
        public GitHubAddModelPage()
            : base(new GitHubModelsViewModel())
        {
            this.InitializeComponent();
        }

        /// <inheritdoc />
        protected override TextBox GetSearchTextBox() => SearchTextBox;

        /// <inheritdoc />
        public override void Search()
        {
            RowHeader.Height = new(1, GridUnitType.Auto);
            base.Search();
        }

        /// <inheritdoc />
        public override void CloseModelInfoViewer() => SearchModelsResultRepeater.CloseModelInfoViewer();

        /// <summary>
        /// Handles the visibility change of the model info viewer.
        /// Adjusts the height of the row header based on visibility.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="eventArgs">The event arguments containing visibility information.</param>
        private void SearchModelsResultRepeater_ModelInfoViewerVisibilityChanged(object _, Components.Controls.ModelInfoViewerVisibilityEventArgs eventArgs)
        {
            RowHeader.Height = eventArgs.IsVisible
                ? new(0, GridUnitType.Pixel)
                : new(1, GridUnitType.Auto);
        }
    }
}