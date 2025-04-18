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

namespace PowerPad.WinUI.ViewModels.AI.Providers
{
    public abstract partial class AIModelsViewModelBase : ObservableObject, IDisposable
    {
        private bool _disposed;
        private bool _searchCompleted;

        protected readonly IAIService _aiService;
        protected readonly SettingsViewModel _settings;
        protected readonly ModelProvider _modelProvider;

        public IRelayCommand<AIModelViewModel> SetDefaultModelCommand { get; }
        public IAsyncRelayCommand<AIModelViewModel> RemoveModelCommand { get; }
        public IAsyncRelayCommand<AIModelViewModel> AddModelCommand { get; }
        public IAsyncRelayCommand<string> SearchModelCommand { get; }

        public ObservableCollection<AIModelViewModel> FilteredModels { get; }

        public ObservableCollection<AIModelViewModel> SearchResultModels { get; }

        [ObservableProperty]
        protected bool _searching;

        public bool FilteredModelsEmpty => !FilteredModels.Any();

        public bool SearchResultModelsEmpty => _searchCompleted && !SearchResultModels.Any();

        public bool RepeaterEnabled { get; protected set; }

        public AIModelsViewModelBase(ModelProvider modelProvider)
        {
            _aiService = App.Get<IAIService>(modelProvider);
            _settings = App.Get<SettingsViewModel>();
            _modelProvider = modelProvider;

            SetDefaultModelCommand = new RelayCommand<AIModelViewModel>(SetDefaultModel);
            RemoveModelCommand = new AsyncRelayCommand<AIModelViewModel>(RemoveModel);
            AddModelCommand = new AsyncRelayCommand<AIModelViewModel>(AddModel);
            SearchModelCommand = new AsyncRelayCommand<string>(SearchModels);

            FilteredModels = 
            [.. 
                _settings.Models.AvailableModels
                    .Where(m => m.ModelProvider == _modelProvider)
                    .OrderBy(m => m.Name) 
            ];

            _settings.General.ProviderAvaibilityChanged += UpdateRepeaterState;
            _settings.Models.AvailableModels.CollectionChanged += FilterModels;

            RepeaterEnabled = _settings.General.AvailableProviders.Contains(_modelProvider);

            SearchResultModels = [];
        }

        protected void FilterModels(object? _, NotifyCollectionChangedEventArgs eventArgs)
        {
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

            OnPropertyChanged(nameof(FilteredModelsEmpty));
        }

        protected void SetDefaultModel(AIModelViewModel? aiModel)
        {
            _settings.Models.DefaultModel = aiModel;
        }

        protected virtual async Task SearchModels(string? query)
        {
            Searching = true;
            _searchCompleted = false;
            OnPropertyChanged(nameof(SearchResultModelsEmpty));

            var resultModels = await _aiService.SearchModels(_modelProvider, query);

            SearchResultModels.Clear();
            SearchResultModels.AddRange(resultModels.Select(m =>
            {
                var existingModel = _settings.Models.AvailableModels
                    .FirstOrDefault(am => am.Name == m.Name && am.ModelProvider == m.ModelProvider);

                return existingModel ?? new AIModelViewModel(m) { Available = false };
            }));

            Searching = false;
            _searchCompleted = true;
            OnPropertyChanged(nameof(SearchResultModelsEmpty));
        }

        protected virtual Task AddModel(AIModelViewModel? aiModel)
        {
            ArgumentNullException.ThrowIfNull(aiModel);

            if (!_settings.Models.AvailableModels.Any(m => m == aiModel))
            {
                aiModel.Available = true;
                aiModel.Enabled = true;

                _settings.Models.AvailableModels.Add(aiModel);
            }

            return Task.CompletedTask;
        }

        protected virtual Task RemoveModel(AIModelViewModel? aiModel)
        {
            ArgumentNullException.ThrowIfNull(aiModel);

            if (_settings.Models.AvailableModels.Any(m => m == aiModel))
            {
                _settings.Models.AvailableModels.Remove(aiModel);
            }

            return Task.CompletedTask;
        }

        protected void UpdateRepeaterState(object? _, EventArgs __)
        {
            RepeaterEnabled = _settings.General.AvailableProviders.Contains(_modelProvider);
            OnPropertyChanged(nameof(RepeaterEnabled));
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
                if (disposing) _settings.Models.AvailableModels.CollectionChanged -= FilterModels;

                _disposed = true;
            }
        }
    }
}