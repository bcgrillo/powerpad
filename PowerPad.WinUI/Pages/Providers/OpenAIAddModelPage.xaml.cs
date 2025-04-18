using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.ViewModels.AI.Providers;

namespace PowerPad.WinUI.Pages.Providers
{
    public partial class OpenAIAddModelPage : AIAddModelPageBase, IModelProviderPage
    {
        public OpenAIAddModelPage()
            : base(new OpenAIModelsViewModel())
        {
            this.InitializeComponent();
        }

        protected override TextBox GetSearchTextBox() => SearchTextBox;

        protected override void Search()
        {
            RowHeader.Height = new(1, GridUnitType.Auto);
            base.Search();
        }

        private void SearchModelsResultRepeater_ModelInfoViewerVisibilityChanged(object _, Components.Controls.ModelInfoViewerVisibilityEventArgs eventArgs)
        {
            RowHeader.Height = eventArgs.IsVisible
                ? new(0, GridUnitType.Pixel)
                : new(1, GridUnitType.Auto);
        }
        public void CloseModelInfoViewer() => SearchModelsResultRepeater.CloseModelInfoViewer();
    }
}