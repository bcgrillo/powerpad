using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Services.AI;
using PowerPad.WinUI.ViewModels.AI;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace PowerPad.WinUI.ViewModels.Settings
{
    public partial class ModelsSettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private AIModelViewModel? _defaultModel;

        [ObservableProperty]
        private bool _sendDefaultParameters;

        public required AIParametersViewModel DefaultParameters
        { 
            get;
            init
            {
                field = value;
                OnDefaultParametersChanged();
                OnPropertyChanged(nameof(DefaultParameters));
            }
        }

        public required ObservableCollection<AIModelViewModel> AvailableModels
        {
            get;
            init
            {
                field = value;
                field.CollectionChanged += AvailableModelsCollectionChangedHandler;
                foreach (var model in field) model.PropertyChanged += AvailableModelsCollectionPropertyChangedHandler;
            }
        }

        public event EventHandler? ModelAvaibilityChanged;
        public event EventHandler? DefaultModelChanged;

        private void AvailableModelsCollectionChangedHandler(object? _, NotifyCollectionChangedEventArgs eventArgs)
        {
            if (eventArgs.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (AIModelViewModel model in eventArgs.NewItems!)
                {
                    model.PropertyChanged += AvailableModelsCollectionPropertyChangedHandler;
                }
            }
            else if (eventArgs.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (AIModelViewModel model in eventArgs.OldItems!)
                {
                    model.PropertyChanged -= AvailableModelsCollectionPropertyChangedHandler;
                }
            }
            else throw new NotImplementedException("Only Add and Remove actions are supported.");

            ModelAvaibilityChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged(nameof(AvailableModels));
        }

        private void AvailableModelsCollectionPropertyChangedHandler(object? sender , PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.PropertyName == nameof(AIModelViewModel.Enabled))
                ModelAvaibilityChanged?.Invoke(this, EventArgs.Empty);

            OnPropertyChanged(nameof(AvailableModels));
        }

        partial void OnDefaultModelChanged(AIModelViewModel? value)
        {
            App.Get<IChatService>().SetDefaultModel(value?.GetRecord());
            DefaultModelChanged?.Invoke(this, EventArgs.Empty);
        }

        partial void OnSendDefaultParametersChanged(bool value)
        {
            App.Get<IChatService>().SetDefaultParameters(value ? DefaultParameters.GetRecord() : null);
        }

        private void OnDefaultParametersChanged()
        {
            App.Get<IChatService>().SetDefaultParameters(SendDefaultParameters ? DefaultParameters.GetRecord() : null);
        }
    }
}