using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Models.AI;
using PowerPad.WinUI.Configuration;
using System.Text.Json.Serialization;

namespace PowerPad.WinUI.ViewModels.AI
{
    public partial class AgentViewModel(Agent agent) : ObservableObject
    {
        private readonly Agent _agent = agent;

        [JsonConstructor]
        public AgentViewModel(string name,
                              string description,
                              string? promptParameterName = null,
                              float? temperature = null,
                              float? topP = null,
                              int? maxOutputTokens = null,
                              AgentIcon? agentIcon = null,
                              string? promptParameterPlaceholder = null)
            : this(new()
            {
                Name = name,
                Description = description,
                PromptParameterName = promptParameterName,
                Temperature = temperature,
                TopP = topP,
                MaxOutputTokens = maxOutputTokens
            })
        {
            _agentIcon = agentIcon;
            _promptParameterPlaceholder = promptParameterPlaceholder;
        }

        public string Name
        {
            get => _agent.Name;
            set => SetProperty(_agent.Name, value, _agent, (x, y) => x.Name = y);
        }

        public string Description
        {
            get => _agent.Description;
            set => SetProperty(_agent.Description, value, _agent, (x, y) => x.Description = y);
        }

        public string? PromptParameterName
        {
            get => _agent.PromptParameterName;
            set => SetProperty(_agent.PromptParameterName, value, _agent, (x, y) => x.PromptParameterName = y);
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
        private AgentIcon? _agentIcon;

        [ObservableProperty]
        private string? _promptParameterPlaceholder;

        public Agent GetRecord() => _agent;
    }
}
