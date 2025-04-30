using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Models.AI;
using System.Text.Json.Serialization;
using PowerPad.WinUI.Helpers;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using PowerPad.WinUI.ViewModels.AI;
using System;
using OllamaSharp.Models;
using Windows.UI;
using PowerPad.WinUI.ViewModels.Settings;

namespace PowerPad.WinUI.ViewModels.Agents
{
    public partial class AgentViewModel(Agent agent, AgentIcon icon) : ObservableObject
    {
        private readonly SettingsViewModel _settings = App.Get<SettingsViewModel>();
        private readonly Agent _agent = agent;

        [JsonConstructor]
        public AgentViewModel(Guid id,
                              string name,
                              string prompt,
                              string? promptParameterName,
                              string? promptParameterDescription,
                              AIModel? aiModel,
                              float? temperature,
                              float? topP,
                              int? maxOutputTokens,
                              AgentIcon icon,
                              bool showInNotes,
                              bool showInChats)
            : this(new()
            {
                Name = name,
                Prompt = prompt,
                PromptParameterName = promptParameterName,
                PromptParameterDescription = promptParameterDescription,
                AIModel = aiModel,
                Temperature = temperature,
                TopP = topP,
                MaxOutputTokens = maxOutputTokens
            }, icon)
        {
            Id = id;
            ShowInNotes = showInNotes;
            ShowInChats = showInChats;
        }

        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name
        {
            get => _agent.Name;
            set => SetProperty(_agent.Name, value, _agent, (x, y) => x.Name = y);
        }

        public string Prompt
        {
            get => _agent.Prompt;
            set => SetProperty(_agent.Prompt, value, _agent, (x, y) => x.Prompt = y);
        }

        public string? PromptParameterName
        {
            get => _agent.PromptParameterName;
            set => SetProperty(_agent.PromptParameterName, value, _agent, (x, y) => x.PromptParameterName = y);
        }

        public string? PromptParameterDescription
        {
            get => _agent.PromptParameterDescription;
            set => SetProperty(_agent.PromptParameterDescription, value, _agent, (x, y) => x.PromptParameterDescription = y);
        }

        public AIModel? AIModel
        {
            get => _agent.AIModel;
            set => SetProperty(_agent.AIModel, value, _agent, (x, y) => x.AIModel = y);
        }

        public float? Temperature
        {
            get => _agent.Temperature;
            set => SetProperty(_agent.Temperature, value, _agent, (x, y) => x.Temperature = y);
        }

        public float? TopP
        {
            get => _agent.TopP;
            set => SetProperty(_agent.TopP, value, _agent, (x, y) => x.TopP = y);
        }

        public int? MaxOutputTokens
        {
            get => _agent.MaxOutputTokens;
            set => SetProperty(_agent.MaxOutputTokens, value, _agent, (x, y) => x.MaxOutputTokens = y);
        }

        [ObservableProperty]
        public partial AgentIcon Icon { get; set; } = icon;

        [ObservableProperty]
        public partial bool ShowInNotes { get; set; } = true;

        [ObservableProperty]
        public partial bool ShowInChats { get; set; } = true;

        [JsonIgnore]
        public bool AllowDropFalse => false; //Allowdrops false only works with binding to a property, not with a constant

        [ObservableProperty]
        [JsonIgnore]
        public partial bool IsSelected { get; set; }

        [JsonIgnore]
        public IconElement IconElement => Icon.Type switch
        {
            AgentIconType.Base64Image => new ImageIcon { Source = Base64ImageHelper.LoadImageFromBase64(Icon.Source) },
            AgentIconType.FontIconGlyph => Icon.Color.HasValue
                ? new FontIcon { Glyph = Icon.Source, Foreground = new SolidColorBrush(Icon.Color.Value) }
                : new FontIcon { Glyph = Icon.Source },
            _ => throw new NotImplementedException()
        };

        [JsonIgnore]
        public bool HasPromptParameter => !string.IsNullOrEmpty(PromptParameterName);

        [JsonIgnore]
        public bool HasAIParameters => Temperature.HasValue;

        public Agent GetRecord() => _agent;

        public void SetRecord(Agent agent)
        {
            Name = agent.Name;
            Prompt = agent.Prompt;
            PromptParameterName = agent.PromptParameterName;
            PromptParameterDescription = agent.PromptParameterDescription;
            AIModel = agent.AIModel;
            Temperature = agent.Temperature;
            TopP = agent.TopP;
            MaxOutputTokens = agent.MaxOutputTokens;
        }

        partial void OnIconChanged(AgentIcon oldValue, AgentIcon newValue) => OnPropertyChanged(nameof(IconElement));

        public AgentViewModel Copy()
        {
            var copy = new AgentViewModel(GetRecord() with { }, Icon) //Shallow copy
            {
                ShowInNotes = ShowInNotes,
                ShowInChats = ShowInChats
            };

            return copy;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not AgentViewModel other)
                return false;

            if (ReferenceEquals(this, other)) return true;

            return GetRecord() == other.GetRecord() &&
                   Icon == other.Icon &&
                   ShowInNotes == other.ShowInNotes &&
                   ShowInChats == other.ShowInChats;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(GetRecord(), Icon, ShowInNotes, ShowInChats);
        }

        public static bool operator ==(AgentViewModel? left, AgentViewModel? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(AgentViewModel? left, AgentViewModel? right)
        {
            return !(left == right);
        }
    }
}
