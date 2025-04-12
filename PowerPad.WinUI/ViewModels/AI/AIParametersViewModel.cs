using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Models.AI;
using System.Text.Json.Serialization;

namespace PowerPad.WinUI.ViewModels.AI
{
    public class AIParametersViewModel(AIParameters aiParameters) : ObservableObject
    {
        private readonly AIParameters _aiParameters = aiParameters;

        [JsonConstructor]
        public AIParametersViewModel(string? systemPrompt = null, float? temperature = null, float? topP = null, int? maxOutputTokens = null, int? maxConversationLength = null)
                   : this(new()
                   {
                       SystemPrompt = systemPrompt,
                       Temperature = temperature,
                       TopP = topP,
                       MaxOutputTokens = maxOutputTokens,
                       MaxConversationLength = maxConversationLength
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

        public float? TopP
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

        public AIParameters GetRecord() => _aiParameters;

        public void Set(AIParameters parameters)
        {
            SystemPrompt = parameters.SystemPrompt;
            Temperature = parameters.Temperature;
            TopP = parameters.TopP;
            MaxOutputTokens = parameters.MaxOutputTokens;
            MaxConversationLength = parameters.MaxConversationLength;
        }

        public override bool Equals(object? other)
        {
            if (other is null) return false;

            if (other is AIParametersViewModel otherAIViewModel)
            {
                if (ReferenceEquals(this, other)) return true;
                return GetRecord() == otherAIViewModel.GetRecord();
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return GetRecord().GetHashCode();
        }

        public static bool operator ==(AIParametersViewModel? left, AIParametersViewModel? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(AIParametersViewModel? left, AIParametersViewModel? right)
        {
            return !(left == right);
        }

        public AIParametersViewModel Copy()
        {
            return new(GetRecord() with { }); //Shallow copy
        }
    }
}
