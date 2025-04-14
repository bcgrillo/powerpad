using System.Text.Json.Serialization;
using System.Text.Json;

namespace PowerPad.Core
{
    public static class Constants
    {
        public static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
}
