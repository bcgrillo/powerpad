using PowerPad.Core.Contracts;

namespace PowerPad.Core.Models.AI
{
    public record AIParameters : IChatOptions
    {
        public string? SystemPrompt { get; set; } = null;
        public float? Temperature { get; set; } = null;
        public float? TopP { get; set; } = null;
        public int? MaxOutputTokens { get; set; } = null;
        public int? MaxConversationLength { get; set; } = null;
    }
}