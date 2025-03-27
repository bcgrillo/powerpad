using System.Text.Json.Serialization;
using System.Collections.ObjectModel;

namespace PowerPad.Core.Services
{
    public enum AIServiceConfigKey
    {
        BaseUrl,
        Key,
    }

    public class AIServiceConfig : ObservableCollection<KeyValuePair<AIServiceConfigKey, object?>>
    {
        private T? Get<T>(AIServiceConfigKey key)
        {
            object? value = this.FirstOrDefault(x => x.Key == key);

            return (T?)value;
        }

        private void Set<T>(AIServiceConfigKey key, T? value)
        {
            var itemsToRemove = this.Where(x => x.Key == key);

            foreach (var i in itemsToRemove) this.Remove(i);

            Add(KeyValuePair.Create(key, (object?)value));
        }

        [JsonIgnore]
        public string? BaseUrl
        {
            get => Get<string?>(AIServiceConfigKey.BaseUrl);
            set => Set(AIServiceConfigKey.BaseUrl, value);
        }

        [JsonIgnore]
        public string? Key
        {
            get => Get<string?>(AIServiceConfigKey.Key);
            set => Set(AIServiceConfigKey.Key, value);
        }
    }
}
