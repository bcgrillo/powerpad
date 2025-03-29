using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Models;
using System.Text.Json.Serialization;

namespace PowerPad.WinUI.ViewModels.AI
{
    public class AIParametersViewModel(AIParameters aiParameters) : ObservableObject
    {
        private readonly AIParameters _aiParameters = aiParameters;

        [JsonConstructor]
        public AIParametersViewModel(string? SystemPrompt, float? Temperature, int? TopP, int? MaxOutputTokens, int? MaxConversationLength)
            : this(new AIParameters
            {
                SystemPrompt = SystemPrompt,
                Temperature = Temperature,
                TopP = TopP,
                MaxOutputTokens = MaxOutputTokens,
                MaxConversationLength = MaxConversationLength
            })
        {
        }

        public string? SystemPrompt
        {
            get => _aiParameters.SystemPrompt;
            set => SetProperty(_aiParameters.SystemPrompt, value, _aiParameters, (x, y) => x.SystemPrompt = y);
        }

        public float? Temperature
        {
            get => _aiParameters.Temperature;
            set => SetProperty(_aiParameters.Temperature, value, _aiParameters, (x, y) => x.Temperature = y);
        }

        public int? TopP
        {
            get => _aiParameters.TopP;
            set => SetProperty(_aiParameters.TopP, value, _aiParameters, (x, y) => x.TopP = y);
        }

        public int? MaxOutputTokens
        {
            get => _aiParameters.MaxOutputTokens;
            set => SetProperty(_aiParameters.MaxOutputTokens, value, _aiParameters, (x, y) => x.MaxOutputTokens = y);
        }

        public int? MaxConversationLength
        {
            get => _aiParameters.MaxConversationLength;
            set => SetProperty(_aiParameters.MaxConversationLength, value, _aiParameters, (x, y) => x.MaxConversationLength = y);
        }

        public AIParameters GetModel() => _aiParameters;
    }
}
