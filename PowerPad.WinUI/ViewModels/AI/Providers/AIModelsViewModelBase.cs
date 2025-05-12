using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;
using PowerPad.WinUI.Helpers;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace PowerPad.WinUI.ViewModels.AI.Providers
{
    /// <summary>
    /// Base class for managing AI models in the application. Provides functionality for filtering, searching, adding, and removing models.
    /// </summary>
    public abstract partial class AIModelsViewModelBase : ObservableObject, IDisposable
    {
        private bool _searchCompleted;

        /// <summary>
        /// The AI service used for searching and managing models.
        /// </summary>
        protected readonly IAIService _aiService;

        /// <summary>
        /// The settings view model containing application settings.
        /// </summary>
        protected readonly SettingsViewModel _settings;

        /// <summary>
        /// The provider of the AI models (e.g., OpenAI, HuggingFace).
        /// </summary>
        protected readonly ModelProvider _modelProvider;

        /// <summary>
        /// Command to set the default AI model.
        /// </summary>
        public IRelayCommand<AIModelViewModel> SetDefaultModelCommand { get; }

        /// <summary>
        /// Command to remove an AI model.
        /// </summary>
        public IAsyncRelayCommand<AIModelViewModel> RemoveModelCommand { get; }

        /// <summary>
        /// Command to add an AI model.
        /// </summary>
        public IAsyncRelayCommand<AIModelViewModel> AddModelCommand { get; }

        /// <summary>
        /// Command to search for AI models.
        /// </summary>
        public IAsyncRelayCommand<string> SearchModelCommand { get; }

        /// <summary>
        /// Collection of filtered AI models based on the current provider.
        /// </summary>
        public ObservableCollection<AIModelViewModel> FilteredModels { get; }

        /// <summary>
        /// Collection of AI models resulting from a search query.
        /// </summary>
        public ObservableCollection<AIModelViewModel> SearchResultModels { get; }

        /// <summary>
        /// Indicates whether a search operation is currently in progress.
        /// </summary>
        [ObservableProperty]
        public partial bool Searching { get; set; }

        /// <summary>
        /// Indicates whether the filtered models collection is empty.
        /// </summary>
        public bool FilteredModelsEmpty => !FilteredModels.Any();

        /// <summary>
        /// Indicates whether the search result models collection is empty.
        /// </summary>
        public bool SearchResultModelsEmpty => _searchCompleted && !SearchResultModels.Any();

        /// <summary>
        /// Indicates whether the repeater functionality is enabled for the current provider.
        /// </summary>
        public bool RepeaterEnabled { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AIModelsViewModelBase"/> class.
        /// </summary>
        /// <param name="modelProvider">The provider of the AI models.</param>
        protected AIModelsViewModelBase(ModelProvider modelProvider)
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

            _settings.General.ProviderAvailabilityChanged += UpdateRepeaterState;
            _settings.Models.AvailableModels.CollectionChanged += FilterModels;

            RepeaterEnabled = _settings.General.AvailableProviders.Contains(_modelProvider);

            SearchResultModels = [];
        }

        /// <summary>
        /// Filters the models based on the current provider and updates the filtered models collection.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="eventArgs">The event arguments containing details about the collection change.</param>
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
                    foreach (var model in eventArgs.OldItems!.OfType<AIModelViewModel>().Where(model => FilteredModels.Contains(model)))
                    {
                        FilteredModels.Remove(model);
                    }
                    break;
                default:
                    FilteredModels.Clear();
                    break;
            }

            OnPropertyChanged(nameof(FilteredModelsEmpty));
        }

        /// <summary>
        /// Sets the specified AI model as the default model.
        /// </summary>
        /// <param name="aiModel">The AI model to set as default.</param>
        protected void SetDefaultModel(AIModelViewModel? aiModel)
        {
            _settings.Models.DefaultModel = aiModel;
        }

        /// <summary>
        /// Searches for AI models based on the specified query.
        /// </summary>
        /// <param name="query">The search query string.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Adds the specified AI model to the available models collection.
        /// </summary>
        /// <param name="aiModel">The AI model to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Removes the specified AI model from the available models collection.
        /// </summary>
        /// <param name="aiModel">The AI model to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual Task RemoveModel(AIModelViewModel? aiModel)
        {
            ArgumentNullException.ThrowIfNull(aiModel);

            if (_settings.Models.AvailableModels.Any(m => m == aiModel))
            {
                _settings.Models.AvailableModels.Remove(aiModel);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Updates the state of the repeater functionality based on the provider availability.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="__">The event arguments (not used).</param>
        protected void UpdateRepeaterState(object? _, EventArgs __)
        {
            RepeaterEnabled = _settings.General.AvailableProviders.Contains(_modelProvider);
            OnPropertyChanged(nameof(RepeaterEnabled));
        }

        /// <summary>
        /// Disposes the resources used by the <see cref="AIModelsViewModelBase"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the resources used by the <see cref="AIModelsViewModelBase"/> class.
        /// </summary>
        /// <param name="disposing">Indicates whether the method is called from the Dispose method.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _settings.General.ProviderAvailabilityChanged -= UpdateRepeaterState;
                _settings.Models.AvailableModels.CollectionChanged -= FilterModels;
            }
        }
    }
}