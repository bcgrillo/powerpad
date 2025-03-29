using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Models;
using PowerPad.WinUI.ViewModels.AI;
using System;
using System.Collections;
using System.Collections.Generic;
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
        private AIParametersViewModel? _defaultParameters;

        public ObservableCollection<AIModelViewModel> AvailableModels { get; }

        public ModelsSettingsViewModel()
        {
            PropertyChanged += PropertyChangedHandler;

            AvailableModels = [];
            AvailableModels.CollectionChanged += CollectionChangedHandler;
        }

        private void PropertyChangedHandler(object? _, PropertyChangedEventArgs eventArgs)
        {
            switch (eventArgs.PropertyName)
            {
                case nameof(DefaultModel):
                    if (DefaultModel != null)
                        DefaultModel.PropertyChanged += (s, e) => OnPropertyChanged($"{nameof(DefaultModel)}.{e.PropertyName}");
                    break;
                case nameof(DefaultParameters):
                    if (DefaultParameters != null)
                        DefaultParameters.PropertyChanged += (s, e) => OnPropertyChanged($"{nameof(DefaultParameters)}.{e.PropertyName}");
                    break;
            }
        }

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
        }

        private void CollectionPropertyChangedHandler(object? _, PropertyChangedEventArgs eventArgs)
        {
            OnPropertyChanged($"{nameof(AvailableModels)}");
        }
    }
}