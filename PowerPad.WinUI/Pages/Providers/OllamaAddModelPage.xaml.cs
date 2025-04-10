using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.ViewModels.AI;

namespace PowerPad.WinUI.Pages.Providers
{
    public partial class OllamaAddModelPage : AIAddModelPageBase
    {
        private OllamaModelsViewModel OllamaModelViewModel => (OllamaModelsViewModel)_modelsViewModel;

        public OllamaAddModelPage()
            : base(new OllamaModelsViewModel())
        {
            this.InitializeComponent();
        }

        protected override TextBox GetSearchTextBox() => SearchTextBox;

        protected override void Search()
        {
            RowHeader.Height = new(1, GridUnitType.Auto);
            base.Search();
        }
    }
}