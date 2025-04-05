using System.Text.Json;
using static PowerPad.Core.Constants;

namespace PowerPad.Core.Services
{
    public interface IConfigStore
    {
        void Set<T>(Enum key, T config);

        T Get<T>(Enum key);

        T? TryGet<T>(Enum key);
    }

    internal class ConfigEntry(string value, bool dirty)
    {
        public string Value { get; set; } = value;
        public bool Dirty { get; set; } = dirty;
    }

    public class ConfigStore : IConfigStore
    {
        private readonly string _configFolder;
        private readonly Dictionary<string, ConfigEntry> _store;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public ConfigStore(string configFolder)
        {
            if (!Directory.Exists(configFolder)) Directory.CreateDirectory(configFolder);

            _configFolder = configFolder;
            _store = [];

            Load();
        }

        public void Set<T>(Enum key, T config)
        {
            _store[key.ToString()] = new(JsonSerializer.Serialize(config, JSON_SERIALIZER_OPTIONS), true);
        }

        public T? TryGet<T>(Enum key)
        {
            try
            {
                if (_store.TryGetValue(key.ToString(), out var config))
                {
                    return JsonSerializer.Deserialize<T>(config.Value, JSON_SERIALIZER_OPTIONS);
                }
            }
            catch (Exception)
            {
                //It's ok
            }

            return default;
        }

        public T Get<T>(Enum key)
        {
            return JsonSerializer.Deserialize<T>(_store[key.ToString()].Value, JSON_SERIALIZER_OPTIONS)
                ?? throw new NullReferenceException($"Config value for key '{key}' is null.");
        }

        private void Load()
        {
            var files = Directory.EnumerateFiles(_configFolder, "*.json");

            foreach (var file in files)
            {
                var key = Path.GetFileNameWithoutExtension(file);
                var config = File.ReadAllText(file);
                _store[key] = new(config, false);
            }
        }

        internal async Task Save()
        {
            await _semaphore.WaitAsync();
            try
            {
                for (var i = 0; i < _store.Count; i++)
                {
                    var (key, value) = _store.ElementAt(i);
                    if (value.Dirty)
                    {
                        var path = Path.Combine(_configFolder, $"{key}.json");
                        await File.WriteAllTextAsync(path, value.Value);
                        value.Dirty = false;
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
