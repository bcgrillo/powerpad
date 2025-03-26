using CommunityToolkit.Mvvm.Input;
using PowerPad.Core.Models;
using PowerPad.Core.Services;
using System.Threading.Tasks;

namespace PowerPad.WinUI.ViewModels
{
    public partial class AzureAIViewModel : AIServiceViewModel
    {
        private readonly IAzureAIService _azureAIService;

        public IRelayCommand RefreshModelsCommand { get; }

        public AzureAIViewModel()
        : base(name: "AzureAI", provider: ModelProvider.GitHub)
        {
            _azureAIService = App.Get<IAzureAIService>();

            RefreshModels();

            RefreshModelsCommand = new RelayCommand(RefreshModels);
        }

        private void RefreshModels()
        {
            var models = _azureAIService.GetModels();

            Models = [];

            foreach (var model in models) Models.Add(new ModelInfoViewModel(model));
        }
    }
}