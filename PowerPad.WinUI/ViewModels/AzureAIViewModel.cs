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

        public AzureAIViewModel(IAzureAIService azureAIService)
        : base(name: "AzureAI", provider: ModelProvider.GitHub)
        {
            _azureAIService = azureAIService;

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