using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PowerPad.Core.Models;
using PowerPad.Core.Services;
using PowerPad.WinUI.ViewModels.Settings;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PowerPad.WinUI.ViewModels.AI
{
    public partial class OpenAIViewModel : ObservableObject
    {
        private readonly IOpenAIService _openAIService;
        private readonly SettingsViewModel _settingsViewModel;

        public IRelayCommand<AIModelViewModel> SetDefaultModelCommand { get; }

        public ObservableCollection<AIModelViewModel> Models { get; }

        public OpenAIViewModel()
        {
            _openAIService = App.Get<IOpenAIService>();
            _settingsViewModel = App.Get<SettingsViewModel>();

            SetDefaultModelCommand = new RelayCommand<AIModelViewModel>(m => _settingsViewModel.Models.DefaultModel = m);

            _settingsViewModel.Models.PropertyChanged += (s, e) => UpdateModels();

            Models = [];
            UpdateModels();
        }

        private void UpdateModels()
        {
            Models.Clear();
            Models.AddRange(_settingsViewModel.Models.AvailableModels.Where(m => m.ModelProvider == ModelProvider.OpenAI));
        }
    }
}