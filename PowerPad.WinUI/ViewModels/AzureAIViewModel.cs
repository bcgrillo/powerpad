using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Models;
using PowerPad.Core.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PowerPad.WinUI.ViewModels
{
    public partial class AzureAIViewModel : ObservableObject
    {
        private readonly IAzureAIService _azureAIService;
        private readonly SettingsViewModel _settingsViewModel;

        public ObservableCollection<AIModelViewModel> Models { get; set; }

        public AzureAIViewModel()
        {
            _azureAIService = App.Get<IAzureAIService>();
            _settingsViewModel = App.Get<SettingsViewModel>();

            Models = 
            [
                .. _settingsViewModel.Models.AvailableModels
                    .Where(m => m.ModelProvider == ModelProvider.GitHub)
                    .Select(m => new AIModelViewModel(m))
            ];
        }
    }
}