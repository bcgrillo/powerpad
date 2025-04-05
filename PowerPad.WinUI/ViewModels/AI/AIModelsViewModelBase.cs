using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;
using PowerPad.Core.Services.AI;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace PowerPad.WinUI.ViewModels.AI
{
    public abstract partial class AIModelsViewModelBase : ObservableObject, IDisposable
    {
        private bool _disposed;

        protected readonly IAIService _aiService;
        protected readonly SettingsViewModel _settingsViewModel;
        protected readonly ModelProvider _modelProvider;

        public IRelayCommand<AIModelViewModel> SetDefaultModelCommand { get; }
        public IRelayCommand<AIModelViewModel> RemoveModelCommand { get; }
        public IRelayCommand<AIModelViewModel> AddModelCommand { get; }
        public IRelayCommand<string> SearchModelCommand { get; }

        public ObservableCollection<AIModelViewModel> FilteredModels { get; }

        public ObservableCollection<AIModelViewModel> SearchResultModels { get; }

        [ObservableProperty]
        private bool _searching;

        public AIModelsViewModelBase(IAIService aiService, ModelProvider modelProvider)
        {
            _aiService = aiService;
            _settingsViewModel = App.Get<SettingsViewModel>();
            _modelProvider = modelProvider;

            SetDefaultModelCommand = new RelayCommand<AIModelViewModel>(SetDefaultModel);
            RemoveModelCommand = new RelayCommand<AIModelViewModel>(m => _settingsViewModel.Models.AvailableModels.Remove(m!));
            AddModelCommand = new RelayCommand<AIModelViewModel>(AddModel);
            SearchModelCommand = new RelayCommand<string>(SearchModels);

            _settingsViewModel.Models.AvailableModels.CollectionChanged += FilterModels;

            FilteredModels = [];
            FilterModels(null, null);

            SearchResultModels = [];
        }

        protected void FilterModels(object? _, NotifyCollectionChangedEventArgs? __)
        {
            var newAvailableModels = _settingsViewModel.Models.AvailableModels;

            foreach (var model in newAvailableModels)
            {
                if (!FilteredModels.Any(m => m == model) && model.ModelProvider == _modelProvider)
                {
                    FilteredModels.Add(model);
                }
            }

            for (int i = FilteredModels.Count - 1; i >= 0; i--)
            {
                var model = FilteredModels[i];
                if (!newAvailableModels.Any(m => m == model) && model.ModelProvider == _modelProvider)
                {
                    FilteredModels.RemoveAt(i);
                }
            }
        }

        private void SetDefaultModel(AIModelViewModel? aiModel)
        {
            _settingsViewModel.Models.DefaultModel = aiModel;
        }

        private async void SearchModels(string? query)
        {
            Searching = true;

            var resultModels = await _aiService.SearchModels(_modelProvider, query);

            SearchResultModels.Clear();
            SearchResultModels.AddRange(resultModels.Select(m =>
            {
                var existingModel = _settingsViewModel.Models.AvailableModels.FirstOrDefault(am => am.GetRecord() == m);

                return new AIModelViewModel
                (
                    m,
                    available: existingModel?.Available ?? false,
                    downloading: existingModel?.Downloading ?? false
                );
            }));

            Searching = false;
        }

        private void AddModel(AIModelViewModel? aiModel)
        {
            ArgumentNullException.ThrowIfNull(aiModel);

            if (!_settingsViewModel.Models.AvailableModels.Any(m => m == aiModel))
            {
                if (aiModel.ModelProvider is ModelProvider.Ollama or ModelProvider.HuggingFace) aiModel.Downloading = true;
                else aiModel.Available = true;

                _settingsViewModel.Models.AvailableModels.Add(aiModel);
            }
        }

        ~AIModelsViewModelBase()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing) _settingsViewModel.Models.AvailableModels.CollectionChanged -= FilterModels;

                _disposed = true;
            }
        }
    }
}