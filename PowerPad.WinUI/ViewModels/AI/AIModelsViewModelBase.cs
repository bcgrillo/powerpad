using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;
using PowerPad.WinUI.ViewModels.Settings;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PowerPad.WinUI.ViewModels.AI
{
    public abstract class AIModelsViewModelBase : ObservableObject
    {
        protected readonly IAIService _aiService;
        protected readonly SettingsViewModel _settingsViewModel;
        protected readonly ModelProvider _modelProvider;

        public IRelayCommand<AIModelViewModel> SetDefaultModelCommand { get; }
        public IRelayCommand<AIModelViewModel> RemoveModelCommand { get; }

        public ObservableCollection<AIModelViewModel> Models { get; }

        public AIModelsViewModelBase(IAIService aiService, ModelProvider modelProvider)
        {
            _aiService = aiService;
            _settingsViewModel = App.Get<SettingsViewModel>();
            _modelProvider = modelProvider;

            SetDefaultModelCommand = new RelayCommand<AIModelViewModel>(m => _settingsViewModel.Models.DefaultModel = m);
            RemoveModelCommand = new RelayCommand<AIModelViewModel>(m => _settingsViewModel.Models.AvailableModels.Remove(m!));

            _settingsViewModel.Models.PropertyChanged += (s, e) => UpdateModels();

            Models = [];
            UpdateModels();
        }

        protected void UpdateModels()
        {
            Models.Clear();
            Models.AddRange(_settingsViewModel.Models.AvailableModels.Where(m => m.ModelProvider == _modelProvider));
        }
    }
}
