using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace PowerPad.WinUI.ViewModels.AI
{
    public abstract partial class AIModelsViewModelBase : ObservableObject, IDisposable
    {
        private bool _disposed;

        protected readonly IAIService _aiService;
        protected readonly SettingsViewModel _settingsViewModel;
        protected readonly ModelProvider _modelProvider;

        public IRelayCommand<AIModelViewModel> SetDefaultModelCommand { get; }
        public IAsyncRelayCommand<AIModelViewModel> RemoveModelCommand { get; }
        public IAsyncRelayCommand<AIModelViewModel> AddModelCommand { get; }
        public IAsyncRelayCommand<string> SearchModelCommand { get; }

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
            RemoveModelCommand = new AsyncRelayCommand<AIModelViewModel>(RemoveModel);
            AddModelCommand = new AsyncRelayCommand<AIModelViewModel>(AddModel);
            SearchModelCommand = new AsyncRelayCommand<string>(SearchModels);

            FilteredModels = [.. _settingsViewModel.Models.AvailableModels.Where(m => m.ModelProvider == _modelProvider)];

            _settingsViewModel.Models.AvailableModels.CollectionChanged += FilterModels;

            SearchResultModels = [];
        }

        protected void FilterModels(object? _, NotifyCollectionChangedEventArgs eventArgs)
        {
            var newAvailableModels = _settingsViewModel.Models.AvailableModels;

            switch (eventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (AIModelViewModel model in eventArgs.NewItems!)
                    {
                        if (model.ModelProvider == _modelProvider && !FilteredModels.Any(m => m == model))
                            FilteredModels.Add(model);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (AIModelViewModel model in eventArgs.OldItems!)
                    {
                        if (FilteredModels.Any(m => m == model)) FilteredModels.Remove(FilteredModels.Single(m => m == model));
                    }
                    break;
                default:
                    FilteredModels.Clear();
                    break;
            }
        }

        private void SetDefaultModel(AIModelViewModel? aiModel)
        {
            _settingsViewModel.Models.DefaultModel = aiModel;
        }

        protected virtual async Task SearchModels(string? query)
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

        protected virtual Task AddModel(AIModelViewModel? aiModel)
        {
            ArgumentNullException.ThrowIfNull(aiModel);

            if (!_settingsViewModel.Models.AvailableModels.Any(m => m == aiModel))
            {
                aiModel.Available = true;

                _settingsViewModel.Models.AvailableModels.Add(aiModel);
            }

            return Task.CompletedTask;
        }

        protected virtual Task RemoveModel(AIModelViewModel? aiModel)
        {
            ArgumentNullException.ThrowIfNull(aiModel);

            _settingsViewModel.Models.AvailableModels.Remove(aiModel);

            return Task.CompletedTask;
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