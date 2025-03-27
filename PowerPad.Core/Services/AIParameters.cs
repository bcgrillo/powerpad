using System.Text.Json.Serialization;
using System.Collections.ObjectModel;

namespace PowerPad.Core.Services
{
    public enum AIParameter
    {
        SystemPrompt,
        Temperature,
        TopP,
        MaxOutputTokens,
        MaxConversationLength,
    }

    public class AIParameters : ObservableCollection<KeyValuePair<AIParameter, object?>>
    {
        private T? Get<T>(AIParameter key)
        {
            object? value = this.FirstOrDefault(x => x.Key == key);

            return (T?)value;
        }

        private void Set<T>(AIParameter key, T? value)
        {
            var itemsToRemove = this.Where(x => x.Key == key);

            foreach(var i in itemsToRemove) this.Remove(i);

            Add(KeyValuePair.Create(key, (object?)value));
        }

        [JsonIgnore]
        public string? SystemPrompt
        {
            get => Get<string?>(AIParameter.SystemPrompt);
            set => Set(AIParameter.SystemPrompt, value);
        }

        [JsonIgnore]
        public float? Temperature
        {
            get => Get<float?>(AIParameter.Temperature);
            set => Set(AIParameter.Temperature, value);
        }

        [JsonIgnore]
        public int? TopP
        {
            get => Get<int?>(AIParameter.TopP);
            set => Set(AIParameter.TopP, value);
        }

        [JsonIgnore]
        public int? MaxOutputTokens
        {
            get => Get<int?>(AIParameter.MaxOutputTokens);
            set => Set(AIParameter.MaxOutputTokens, value);
        }

        [JsonIgnore]
        public int? MaxConversationLength
        {
            get => Get<int?>(AIParameter.MaxConversationLength);
            set => Set(AIParameter.MaxConversationLength, value);
        }
    }
}
