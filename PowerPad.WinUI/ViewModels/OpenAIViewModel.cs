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

        public OpenAIViewModel()
        : base(name: "OpenAI", provider: ModelProvider.OpenAI)
        {
            _openAIService = App.Get<IOpenAIService>();

            RefreshModels();

            RefreshModelsCommand = new RelayCommand(RefreshModels);
        }

        private void RefreshModels()
        {
            var models = _openAIService.GetModels();

            Models = [];

            foreach (var model in models) Models.Add(new ModelInfoViewModel(model));
        }
    }
}