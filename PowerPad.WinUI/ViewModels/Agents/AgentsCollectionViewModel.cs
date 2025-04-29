using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Services.Config;
using PowerPad.WinUI.ViewModels.AI;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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

        public event EventHandler? AgentsAvaibilityChanged;

        public AgentsCollectionViewModel()
        {
            _configStore = App.Get<IConfigStore>();

            Agents = _configStore.Get<ObservableCollection<AgentViewModel>>(StoreKey.Agents);
        }

        public AgentViewModel? GetAgent(Guid id) => Agents.FirstOrDefault(x => x.Id == id);

        private void CollectionChangedHandler(object? _, NotifyCollectionChangedEventArgs eventArgs)
        {
            OnPropertyChanged($"{nameof(Agents)}");

            switch (eventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (AgentViewModel model in eventArgs.NewItems!)
                    {
                        model.PropertyChanged += CollectionPropertyChangedHandler;
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (AgentViewModel model in eventArgs.OldItems!)
                    {
                        model.PropertyChanged -= CollectionPropertyChangedHandler;
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                default:
                    throw new NotImplementedException("Only Add and Remove actions are supported.");
            }

            AgentsAvaibilityChanged?.Invoke(null, EventArgs.Empty);
            SaveAgents();
        }

        private void CollectionPropertyChangedHandler(object? _, PropertyChangedEventArgs eventArgs)
        {
            AgentsAvaibilityChanged?.Invoke(null, EventArgs.Empty);
            SaveAgents();
        }

        private void SaveAgents() => _configStore.Set(StoreKey.Agents, Agents);
    }
}
