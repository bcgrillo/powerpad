using PowerPad.Core.Contracts;

namespace PowerPad.Core.Models.AI
{
    public record Agent() : AgentParameters
    {
        public required string Name { get; set; }

        public required string Prompt { get; set; }

        public string? PromptParameterName { get; set; } = null;

        public string? PromptParameterDescription { get; set; } = null;

        public AIModel? AIModel { get; set; }

        internal IChatOptions GetParameters(AIParameters? defaultParameters)
        {
            return new AgentParameters
            {
                Temperature = Temperature ?? defaultParameters?.Temperature,
                TopP = TopP ?? defaultParameters?.TopP,
                MaxOutputTokens = MaxOutputTokens ?? defaultParameters?.MaxOutputTokens
            };
        }
    }

    public record AgentParameters : IChatOptions
    {
        public float? Temperature { get; set; } = null;

        public float? TopP { get; set; } = null;

        public int? MaxOutputTokens { get; set; } = null;
    }
}