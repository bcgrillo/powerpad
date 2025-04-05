using CommunityToolkit.Mvvm.ComponentModel;
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
                field.PropertyChanged += SecondaryPropertyChangedHandler;
            }
        }

        public required ObservableCollection<AIModelViewModel> AvailableModels
        {
            get;
            init
            {
                field = value;
                field.CollectionChanged += CollectionChangedHandler;
                foreach (AIModelViewModel model in field) model.PropertyChanged += CollectionPropertyChangedHandler;
            }
        }

        public ModelsSettingsViewModel()
        {
            PropertyChanged += PropertyChangedHandler;
        }

        private void PropertyChangedHandler(object? _, PropertyChangedEventArgs eventArgs)
        {
            switch (eventArgs.PropertyName)
            {
                case nameof(DefaultModel):
                    if (DefaultModel is not null)
                        DefaultModel.PropertyChanged += SecondaryPropertyChangedHandler;
                    break;
            }
        }

        private void SecondaryPropertyChangedHandler(object? _, PropertyChangedEventArgs __) => OnPropertyChanged();

        private void CollectionChangedHandler(object? _, NotifyCollectionChangedEventArgs eventArgs)
        {
            OnPropertyChanged($"{nameof(AvailableModels)}");

            if (eventArgs.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (AIModelViewModel model in eventArgs.NewItems!)
                {
                    model.PropertyChanged += CollectionPropertyChangedHandler;
                }
            }
            else if (eventArgs.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (AIModelViewModel model in eventArgs.OldItems!)
                {
                    model.PropertyChanged -= CollectionPropertyChangedHandler;
                }
            }
            else throw new NotImplementedException("Only Add and Remove actions are supported.");

            ValidateDefaultModel();
        }

        private void CollectionPropertyChangedHandler(object? _, PropertyChangedEventArgs __) => OnPropertyChanged();

        private void ValidateDefaultModel()
        {
            if (DefaultModel is null || !AvailableModels.Contains(DefaultModel)) DefaultModel = null;
        }
    }
}