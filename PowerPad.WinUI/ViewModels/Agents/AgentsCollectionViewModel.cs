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
        private static readonly string[] RANDOM_GLYPHS =
        [
            "\uE99A", "\uE70F", "\uE734", "\uE74C", "\uE76E", "\uE774", "\uE7C1", "\uE7E6",
            "\uE805", "\uE81B", "\uE82F", "\uE897", "\uE8C6", "\uE8C9", "\uE8EC", "\uE909",
            "\uE932", "\uE943", "\uE945", "\uE950", "\uE98F", "\uE9F5", "\uEB50", "\uEA91",
            "\uEB51", "\uEBE8", "\uEC26", "\uEC32", "\uECAD", "\uED15", "\uED63", "\uEDC6",
            "\uEE56", "\uF0B9", "\uF0E3", "\uF133", "\uF156", "\uF22C", "\uF384", "\uF49A",
            "\uF4A5", "\uF4AA", "\uF6B8", "\uF7BB", "\uF87B", "\uF2B7"
        ];

        private readonly SettingsViewModel _settings;
        private readonly IConfigStore _configStore;
        private int _currentGlyphIndex = 0;

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

            var actualGlyphIndex = _currentGlyphIndex % RANDOM_GLYPHS.Length;
            _currentGlyphIndex++;
            var glyph = RANDOM_GLYPHS[actualGlyphIndex];

            return new(glyph, AgentIconType.FontIconGlyph, color);
        }

        private void SaveAgents() => _configStore.Set(StoreKey.Agents, Agents);
    }
}
