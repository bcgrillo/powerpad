using System.Text.Json.Serialization;
using System.Collections.ObjectModel;

namespace PowerPad.Core.Models.AI
{
    public record AIServiceConfig
    {
        public string? BaseUrl { get; set; } = null;
        public string? Key { get; set; } = null;
    }
}