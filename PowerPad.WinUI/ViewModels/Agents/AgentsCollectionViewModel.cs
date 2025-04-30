using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using PowerPad.Core.Services.Config;
using PowerPad.WinUI.ViewModels.AI;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Windows.UI;
using static PowerPad.WinUI.Configuration.ConfigConstants;

namespace PowerPad.WinUI.ViewModels.Agents
{
    public class AgentsCollectionViewModel : ObservableObject
    {
        private readonly SettingsViewModel _settings;
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
            _settings = App.Get<SettingsViewModel>();
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

        public AgentIcon GenerateIcon()
        {
            var mode = _settings.General.AppTheme ?? Application.Current.RequestedTheme;
            var random = new Random();
            Color color;

            if (mode == ApplicationTheme.Dark)
            {
                color = Color.FromArgb(255,
                    (byte)random.Next(50, 250),
                    (byte)random.Next(50, 250),
                    (byte)random.Next(50, 250));
            }
            else
            {
                color = Color.FromArgb(255,
                    (byte)random.Next(0, 200),
                    (byte)random.Next(0, 200),
                    (byte)random.Next(0, 200));
            }

            int brightness = color.R + color.G + color.B;

            if (mode == ApplicationTheme.Dark && brightness < 400)
            {
                color = Color.FromArgb(255,
                    (byte)Math.Min(color.R + 50, 255),
                    (byte)Math.Min(color.G + 50, 255),
                    (byte)Math.Min(color.B + 50, 255));
            }
            else if (mode == ApplicationTheme.Light && brightness > 200)
            {
                color = Color.FromArgb(255,
                    (byte)Math.Max(color.R - 50, 0),
                    (byte)Math.Max(color.G - 50, 0),
                    (byte)Math.Max(color.B - 50, 0));
            }

            return new("\uE99A", AgentIconType.FontIconGlyph, color);
        }

        private void SaveAgents() => _configStore.Set(StoreKey.Agents, Agents);
    }
}
