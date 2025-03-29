using System.Text.Json.Serialization;
using System.Collections.ObjectModel;

namespace PowerPad.Core.Models
{
    public class AIServiceConfig
    {
        public string? BaseUrl { get; set; }

        public string? Key { get; set; }
    }
}
