using System.Text.Json.Serialization;

namespace PowerPad.Core.Models.AI
{
    public class AIModel
    (
        string name,
        ModelProvider modelProvider,
        long? size = null,
        string? displayName = null
    )
    {
        public string Name { get; init; } = name;
        public ModelProvider ModelProvider { get; init; } = modelProvider;

        public long? Size { get; set; } = size;
        public string? DisplayName { get; } = displayName;

        public override bool Equals(object? other)
        {
            if (other is null) return false;

            if (other is AIModel otherAIModel)
            {
                if (ReferenceEquals(this, other)) return true;
                return Name == otherAIModel.Name && ModelProvider == otherAIModel.ModelProvider;
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, ModelProvider);
        }

        public static bool operator ==(AIModel? left, AIModel? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(AIModel? left, AIModel? right)
        {
            return !(left == right);
        }
    }
}