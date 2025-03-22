using CommunityToolkit.Mvvm.Input;
using PowerPad.Core.Models;
using PowerPad.Core.Services;
using System.Threading.Tasks;

namespace PowerPad.WinUI.ViewModels
{
    public partial class OpenAIViewModel : AIServiceViewModel
    {
        private readonly IOpenAIService _openAIService;

        public IRelayCommand RefreshModelsCommand { get; }

        public OpenAIViewModel(IOpenAIService openAIService)
        : base(name: "OpenAI", provider: ModelProvider.OpenAI)
        {
            _openAIService = openAIService;

            _ = RefreshModels();

            RefreshModelsCommand = new RelayCommand(async () => await RefreshModels());
        }

        private async Task RefreshModels()
        {
            var models = await _openAIService.GetModels();

            Models = [];

            foreach (var model in models) Models.Add(new ModelInfoViewModel(model));
        }
    }
}