using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.WinUI.ViewModels.AI;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

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
                field.CollectionChanged += AvailableCollectionChangedHandler;
                foreach (AIModelViewModel model in field) model.PropertyChanged += AvailableCollectionPropertyChangedHandler;
            }
        }

        public required ObservableCollection<AIModelViewModel> RecoverableModels
        {
            get;
            init
            {
                field = value;
                field.CollectionChanged += RecoverableCollectionChangedHandler;
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

        private void AvailableCollectionChangedHandler(object? _, NotifyCollectionChangedEventArgs eventArgs)
        {
            if (eventArgs.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (AIModelViewModel model in eventArgs.NewItems!)
                {
                    model.PropertyChanged += AvailableCollectionPropertyChangedHandler;
                }
            }
            else if (eventArgs.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (AIModelViewModel model in eventArgs.OldItems!)
                {
                    model.PropertyChanged -= AvailableCollectionPropertyChangedHandler;
                }
            }
            else throw new NotImplementedException("Only Add and Remove actions are supported.");

            OnPropertyChanged(nameof(AvailableModels));
        }

        private void AvailableCollectionPropertyChangedHandler(object? _, PropertyChangedEventArgs __)
        {
            OnPropertyChanged(nameof(AvailableModels));
        }

        private void RecoverableCollectionChangedHandler(object? _, NotifyCollectionChangedEventArgs eventArgs)
        {
            OnPropertyChanged(nameof(RecoverableModels));
        }
    }
}