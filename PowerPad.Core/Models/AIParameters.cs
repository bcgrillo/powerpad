using System.Text.Json.Serialization;
using System.Collections.ObjectModel;

namespace PowerPad.Core.Models
{
    public class AIParameters
    {
        public string? SystemPrompt { get; set; }

        public float? Temperature { get; set; }

        public int? TopP { get; set; }

        public int? MaxOutputTokens { get; set; }

        public int? MaxConversationLength { get; set; }
    }
}
