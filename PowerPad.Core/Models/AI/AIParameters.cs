using System.Text.Json.Serialization;
using System.Collections.ObjectModel;

namespace PowerPad.Core.Models.AI
{
    public record AIParameters
    {
        public string? SystemPrompt { get; set; }

        public float? Temperature { get; set; }

        public float? TopP { get; set; }

        public int? MaxOutputTokens { get; set; }

        public int? MaxConversationLength { get; set; }
    }
}
