using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Models;
using PowerPad.Core.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PowerPad.WinUI.ViewModels
{
    public partial class OpenAIViewModel : ObservableObject
    {
        private readonly IOpenAIService _openAIService;
        private readonly SettingsViewModel _settingsViewModel;

        public ObservableCollection<AIModelViewModel> Models { get; set; }

        public OpenAIViewModel()
        {
            _openAIService = App.Get<IOpenAIService>();
            _settingsViewModel = App.Get<SettingsViewModel>();

            Models =
            [
                .. _settingsViewModel.Models.AvailableModels
                    .Where(m => m.ModelProvider == ModelProvider.OpenAI)
                    .Select(m => new AIModelViewModel(m))
            ];
        }
    }
}