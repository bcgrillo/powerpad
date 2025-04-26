using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Services.Config;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using static PowerPad.WinUI.Configuration.ConfigConstants;

namespace PowerPad.WinUI.ViewModels.Agents
{
    public class AgentsCollectionViewModel : ObservableObject
    {
        private readonly IConfigStore _configStore;

        public required ObservableCollection<AgentViewModel> Agents
        {
            get;
            init
            {
                field = value;
                field.CollectionChanged += CollectionChangedHandler;
                foreach (AgentViewModel model in field) model.PropertyChanged += CollectionPropertyChangedHandler;
            }
        }

        public AgentsCollectionViewModel()
        {
            _configStore = App.Get<IConfigStore>();

            Agents = _configStore.Get<ObservableCollection<AgentViewModel>>(StoreKey.Agents);
        }

        private void CollectionChangedHandler(object? _, NotifyCollectionChangedEventArgs eventArgs)
        {
            OnPropertyChanged($"{nameof(Agents)}");

            if (eventArgs.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (AgentViewModel model in eventArgs.NewItems!)
                {
                    model.PropertyChanged += CollectionPropertyChangedHandler;
                }
            }
            else if (eventArgs.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (AgentViewModel model in eventArgs.OldItems!)
                {
                    model.PropertyChanged -= CollectionPropertyChangedHandler;
                }
                    break;
                default:
                    if (eventArgs.Action != NotifyCollectionChangedAction.Move)
                throw new NotImplementedException("Only Add and Remove actions are supported.");

            SaveAgents();
        }

        private void CollectionPropertyChangedHandler(object? _, PropertyChangedEventArgs __) => SaveAgents();

        private void SaveAgents() => _configStore.Set(StoreKey.Agents, Agents);
    }
}
