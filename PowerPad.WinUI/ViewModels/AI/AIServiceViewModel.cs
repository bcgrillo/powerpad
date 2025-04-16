using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.WinUI.ViewModels.AI
{
    public partial class AIServiceViewModel<TService> : ObservableObject
        where TService : IAIService
    {
        [ObservableProperty]
        private bool _enabled;

        [ObservableProperty]
        private bool _online;

        [ObservableProperty]
        private bool _hasError;

        [ObservableProperty]
        private string? _errorMessage;

        public required AIServiceConfigViewModel Config;

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

        public async Task<TestConnectionResult> TestConnection()
        {
            if (Enabled)
            {
                var service = App.Get<TService>();

                var test = await service.TestConection();

                HasError = !test.Success;
                ErrorMessage = test.ErrorMessage;
                return test;
            }
            else
            {
                return new TestConnectionResult(false, "Service is not enabled.");
            }
        }

        private void CollectionChangedHandler(object? _, NotifyCollectionChangedEventArgs eventArgs)
        {
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

            OnPropertyChanged(nameof(AvailableModels));
        }

        private void CollectionPropertyChangedHandler(object? _, PropertyChangedEventArgs __)
        {
            OnPropertyChanged(nameof(AvailableModels));
        }
    }
}
