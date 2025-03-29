using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PowerPad.Core.Models;
using PowerPad.Core.Services;
using PowerPad.WinUI.ViewModels.AI;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PowerPad.WinUI.ViewModels
{
    public partial class OllamaViewModel : ObservableObject
    {
        private readonly IOllamaService _ollamaService;
        private readonly SettingsViewModel _settingsViewModel;

        [ObservableProperty]
        private OllamaStatus _ollamaStatus;

        public IRelayCommand RefreshStatusCommand { get; }
        public IRelayCommand RefreshModelsCommand { get; }
        public IRelayCommand<AIModelViewModel> SetDefaultModelCommand { get; }


        public ObservableCollection<AIModelViewModel> OllamaModels { get; }

        public ObservableCollection<AIModelViewModel> HuggingFaceModels { get; }

        public OllamaViewModel()
        {
            _ollamaService = App.Get<IOllamaService>();
            _settingsViewModel = App.Get<SettingsViewModel>();

            RefreshStatusCommand = new RelayCommand(async () => await RefreshStatus());
            RefreshModelsCommand = new RelayCommand(async () => await RefreshModels());
            SetDefaultModelCommand = new RelayCommand<AIModelViewModel>(m => _settingsViewModel.Models.DefaultModel = m);
            
            _ollamaStatus = OllamaStatus.Unknown;
            _ = RefreshStatus();

            _settingsViewModel.Models.PropertyChanged += (s, e) => UpdateModels();

            OllamaModels = [];
            HuggingFaceModels = [];
            UpdateModels();
        }

        private void UpdateModels()
        {
            OllamaModels.Clear();
            OllamaModels.AddRange(_settingsViewModel.Models.AvailableModels.Where(m => m.ModelProvider == ModelProvider.Ollama));

            HuggingFaceModels.Clear();
            HuggingFaceModels.AddRange(_settingsViewModel.Models.AvailableModels.Where(m => m.ModelProvider == ModelProvider.HuggingFace));
        }

        private async Task RefreshStatus()
        {
            OllamaStatus = await _ollamaService.GetStatus();
        }

        private async Task RefreshModels()
        {
            await RefreshStatus();

            IEnumerable<AIModel> newAvailableModels;

            if (OllamaStatus == OllamaStatus.Online)
            {
                newAvailableModels = await _ollamaService.GetAvailableModels();
            }
            else
            {
                newAvailableModels = [];
            }

            var currentAvailableModels = _settingsViewModel.Models.AvailableModels;

            foreach (var model in newAvailableModels)
            {
                if (!currentAvailableModels.Any(m => m.GetModel() == model))
                {
                    currentAvailableModels.Add(new(model));
                }
            }

            for (int i = currentAvailableModels.Count - 1; i >= 0; i--)
            {
                var model = currentAvailableModels[i];
                if (!newAvailableModels.Any(m => m == model.GetModel()) &&
                    (model.ModelProvider == ModelProvider.GitHub || model.ModelProvider == ModelProvider.HuggingFace))
                {
                    currentAvailableModels.RemoveAt(i);
                }
            }

            UpdateModels();
        }
    }
}