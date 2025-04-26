using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Models.AI;
using System.Text.Json.Serialization;
using PowerPad.WinUI.Helpers;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;

namespace PowerPad.WinUI.ViewModels.Agents
{
    public partial class AgentViewModel(Agent agent) : ObservableObject
    {
        private readonly Agent _agent = agent;

        [JsonConstructor]
        public AgentViewModel(string name,
                              string description,
                              string? promptParameterName = null,
                              string? promptParameterDescription = null,
                              AIModel? aiModel = null,
                              float? temperature = null,
                              float? topP = null,
                              int? maxOutputTokens = null,
                              AgentIcon? agentIcon = null)
            : this(new()
            {
                Name = name,
                Prompt = description,
                PromptParameterName = promptParameterName,
                PromptParameterDescription = promptParameterDescription,
                AIModel = aiModel,
                Temperature = temperature,
                TopP = topP,
                MaxOutputTokens = maxOutputTokens
            })
        {
            AgentIcon = agentIcon;
        }

        public string Name
        {
            get => _agent.Name;
            set => SetProperty(_agent.Name, value, _agent, (x, y) => x.Name = y);
        }

        public string Description
        {
            get => _agent.Prompt;
            set => SetProperty(_agent.Prompt, value, _agent, (x, y) => x.Prompt = y);
        }

        public string? PromptParameterName
        {
            get => _agent.PromptParameterName;
            set
            {
                SetProperty(_agent.PromptParameterName, value, _agent, (x, y) => x.PromptParameterName = y);
                OnPropertyChanged(nameof(HasPromptParameter));
            }
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
        public partial AgentIcon? AgentIcon { get; set; }

        [ObservableProperty]
        public partial bool Enabled { get; set; }

        [JsonIgnore]
        public bool AllowDropFalse => false; //Allowdrops false only works with binding to a property, not with a constant

        [JsonIgnore]
        public IconElement? IconElement => AgentIcon?.IconType switch
        {
            AgentIconType.Base64Image => new ImageIcon { Source = Base64ImageHelper.LoadImageFromBase64(AgentIcon.Value.IconSource), HorizontalAlignment = HorizontalAlignment.Center },
            AgentIconType.CharacterOrEmoji => new FontIcon { Glyph = AgentIcon.Value.IconSource, HorizontalAlignment = HorizontalAlignment.Center, Margin = new(-4, -2, 0, 0), IsTextScaleFactorEnabled = true, FontFamily = (FontFamily)Application.Current.Resources["ContentControlThemeFontFamily"] },
            AgentIconType.FontIconGlyph => new FontIcon { Glyph = AgentIcon.Value.IconSource, HorizontalAlignment = HorizontalAlignment.Center },
            _ => null,
        };

        [JsonIgnore]
        public bool HasPromptParameter => !string.IsNullOrEmpty(PromptParameterName);

        public Agent GetRecord() => _agent;

        partial void OnAgentIconChanged(AgentIcon? oldValue, AgentIcon? newValue) => OnPropertyChanged(nameof(IconElement));
    }
}
