using static PowerPad.Core.Constants.AgentConfig;

namespace PowerPad.Core.Models.AI
{
    public record Agent()
    {
        public required string Name { get; set; }

        public required string Description { get; set; }

        public string? PromptParameterName { get; set; } = null;

        public float? Temperature { get; set; } = null;

        public float? TopP { get; set; } = null;

        public int? MaxOutputTokens { get; set; } = null;

        public AIParameters GetAgentParameters(string? parameterValue)
        {
            var systemPrompt = AgentPromptFormatPart1
                .Replace(PromptReplaceToken.Description.ToString(), Description);

            if (PromptParameterName is not null)
            {
                systemPrompt += AgentPromptFormatPart2
                    .Replace(PromptReplaceToken.ParameterName.ToString(), PromptParameterName)
                    .Replace(PromptReplaceToken.ParameterValue.ToString(), parameterValue);
            }

            systemPrompt += AgentPromptFormatPart3;

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