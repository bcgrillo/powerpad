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


        public class AgentConfig
        {
            public enum PromptReplaceToken
            {
                Description,
                ParameterName,
                ParameterValue
            }

            public static string AgentPromptFormatPart1 =
                $"Eres un agente de inteligencia artificial personalizado.\n" +
                $"Esta es tu descripción: {{{PromptReplaceToken.Description}}}\n";

            public static string AgentPromptFormatPart2 =
                $"Debes tener en cuenta el siguiente parámetro para realizar tu tarea:\n" +
                $"{{{PromptReplaceToken.ParameterName}}}: {{{PromptReplaceToken.ParameterValue}}}\n";
            
            public static string AgentPromptFormatPart3 =
                $"IMPORTANTE:\n" +
                $"- El único input que recibirás será el mensaje del usuario.\n" +
                $"- Tu respuesta debe ser el texto completo modificado según el objetivo, manteniendo intacto todo lo que no sea necesario cambiar.\n" +
                $"- No incluyas explicaciones, saludos ni mensajes adicionales.";
        }
    }
}
