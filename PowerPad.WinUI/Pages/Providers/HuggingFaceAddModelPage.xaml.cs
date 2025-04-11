using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.ViewModels.AI;

namespace PowerPad.WinUI.Pages.Providers
{
    public partial class HuggingFaceAddModelPage : AIAddModelPageBase
    {
        private HuggingFaceModelsViewModel HuggingFaceModelsViewModel => (HuggingFaceModelsViewModel)_modelsViewModel;

        public HuggingFaceAddModelPage()
            : base(new HuggingFaceModelsViewModel())
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