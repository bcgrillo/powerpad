using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Services.AI;
using PowerPad.WinUI.ViewModels.AI;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace PowerPad.WinUI.ViewModels.Settings
{
    /// <summary>
    /// ViewModel for managing settings related to AI models, including default model, parameters, and available models.
    /// </summary>
    public partial class ModelsSettingsViewModel : ObservableObject
    {
        /// <summary>
        /// Gets or sets the default AI model.
        /// </summary>
        [ObservableProperty]
        public partial AIModelViewModel? DefaultModel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether default parameters should be sent.
        /// </summary>
        [ObservableProperty]
        public partial bool SendDefaultParameters { get; set; }

        /// <summary>
        /// Gets the default parameters for the AI model.
        /// </summary>
        /// <remarks>
        /// This property is required and initialized during object creation.
        /// </remarks>
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

        /// <summary>
        /// Gets the collection of available AI models.
        /// </summary>
        /// <remarks>
        /// This property is required and initialized during object creation.
        /// </remarks>
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

        /// <summary>
        /// Event triggered when the availability of models changes.
        /// </summary>
        public event EventHandler? ModelAvailabilityChanged;

        /// <summary>
        /// Event triggered when the default model changes.
        /// </summary>
        public event EventHandler? DefaultModelChanged;

        /// <summary>
        /// Called when the default model changes.
        /// </summary>
        /// <param name="value">The new default model.</param>
        partial void OnDefaultModelChanged(AIModelViewModel? value)
        {
            App.Get<IChatService>().SetDefaultModel(value?.GetRecord());
            DefaultModelChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the SendDefaultParameters property changes.
        /// </summary>
        /// <param name="value">The new value of the SendDefaultParameters property.</param>
        partial void OnSendDefaultParametersChanged(bool value)
        {
            App.Get<IChatService>().SetDefaultParameters(value ? DefaultParameters.GetRecord() : null);
        }

        /// <summary>
        /// Updates the default parameters in the chat service.
        /// </summary>
        private void OnDefaultParametersChanged()
        {
            App.Get<IChatService>().SetDefaultParameters(SendDefaultParameters ? DefaultParameters.GetRecord() : null);
        }

        /// <summary>
        /// Handles changes to the collection of available models.
        /// </summary>
        /// <param name="_">The source of the event (not used).</param>
        /// <param name="eventArgs">The event data containing details of the collection change.</param>
        private void AvailableModelsCollectionChangedHandler(object? _, NotifyCollectionChangedEventArgs eventArgs)
        {
            switch (eventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (AIModelViewModel model in eventArgs.NewItems!)
                    {
                        model.PropertyChanged += AvailableModelsCollectionPropertyChangedHandler;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (AIModelViewModel model in eventArgs.OldItems!)
                    {
                        model.PropertyChanged -= AvailableModelsCollectionPropertyChangedHandler;
                    }
                    break;
                default:
                    throw new NotImplementedException("Only Add and Remove actions are supported.");
            }

            ModelAvailabilityChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged(nameof(AvailableModels));
        }

        /// <summary>
        /// Handles property changes within the available models collection.
        /// </summary>
        /// <param name="_">The source of the event (not used).</param>
        /// <param name="eventArgs">The event data containing details of the property change.</param>
        private void AvailableModelsCollectionPropertyChangedHandler(object? _, PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.PropertyName == nameof(AIModelViewModel.Enabled))
                ModelAvailabilityChanged?.Invoke(this, EventArgs.Empty);

            OnPropertyChanged(nameof(AvailableModels));
        }
    }
}