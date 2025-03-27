using System.Text.Json.Serialization;

namespace PowerPad.Core.Models
{
    public class AIModel
    (
        string name,
        ModelProvider modelProvider,
        bool enabled = false,
        long? size = null,
        string? displayName = null
    )
    {
        public string Name { get; init; } = name;
        public ModelProvider ModelProvider { get; init; } = modelProvider;

        public bool Enabled { get; set; } = enabled;
        public long? Size { get; set; } = size;
        public string? DisplayName { get; set; } = displayName;


        public virtual bool Equals(AIModel? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name && ModelProvider == other.ModelProvider;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, ModelProvider);
        }
    }
}