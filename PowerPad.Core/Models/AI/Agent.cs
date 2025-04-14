namespace PowerPad.Core.Models.AI
{
    public record Agent()
    {
        public required string Name { get; set; }

        public required string Prompt { get; set; }

        public string? PromptParameterName { get; set; } = null;

        public string? PromptParameterDescription { get; set; } = null;

        public AIModel? AIModel { get; set; }

        public float? Temperature { get; set; } = null;

        public float? TopP { get; set; } = null;

        public int? MaxOutputTokens { get; set; } = null;

        public AIParameters GetAgentParameters(string? parameterValue, string? aditionalPrompt)
        {
            var systemPrompt = Prompt;

            if (PromptParameterName is not null)
                systemPrompt += $"\n{PromptParameterName}: {parameterValue}";

            if (aditionalPrompt is not null)
                systemPrompt += $"\n{aditionalPrompt}";

            return new AIParameters
            {
                SystemPrompt = systemPrompt,
                Temperature = Temperature,
                TopP = TopP,
                MaxOutputTokens = MaxOutputTokens
            };
        }
    }
}