using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace PowerPad.WinUI.ViewModels.AI
{
    public abstract class AIModelsViewModelBase : ObservableObject, IDisposable
    {
        private bool _disposed;

        protected readonly IAIService _aiService;
        protected readonly SettingsViewModel _settingsViewModel;
        protected readonly ModelProvider _modelProvider;

        public IRelayCommand<AIModelViewModel> SetDefaultModelCommand { get; }
        public IRelayCommand<AIModelViewModel> RemoveModelCommand { get; }

        public ObservableCollection<AIModelViewModel> FilteredModels { get; }

        public AIModelsViewModelBase(IAIService aiService, ModelProvider modelProvider)
        {
            _aiService = aiService;
            _settingsViewModel = App.Get<SettingsViewModel>();
            _modelProvider = modelProvider;

            SetDefaultModelCommand = new RelayCommand<AIModelViewModel>(m => _settingsViewModel.Models.DefaultModel = m);
            RemoveModelCommand = new RelayCommand<AIModelViewModel>(m => _settingsViewModel.Models.AvailableModels.Remove(m!));

            _settingsViewModel.Models.AvailableModels.CollectionChanged += FilterModels;

            FilteredModels = [];
            FilterModels(null, null);
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

            var x = FilteredModels.Count;
            for (int i = x - 1; i >= 0; i--)
            {
                var model = FilteredModels[i];
                if (!newAvailableModels.Any(m => m == model) && model.ModelProvider == _modelProvider)
                {
                    FilteredModels.RemoveAt(i);
                }
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