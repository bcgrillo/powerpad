using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PowerPad.Core.Configuration
{
    public interface IConfigStore
    {
        void Set<T>(string key, T config);

        T Get<T>(string key);

        T? TryGet<T>(string key);
    }

    internal struct ConfigEntry(string value, bool dirty)
    {
        public string Value { get; set; } = value;
        public bool Dirty { get; set; } = dirty;
    }

    public class ConfigStore : IConfigStore
    {
        private readonly string _configFolder;
        private readonly Dictionary<string, ConfigEntry> _store;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public ConfigStore(string configFolder)
        {
            if (!Directory.Exists(configFolder)) Directory.CreateDirectory(configFolder);

            _configFolder = configFolder;
            _store = new Dictionary<string, ConfigEntry>();

            Load();
        }

        public void Set<T>(string key, T config)
        {
            _store[key] = new ConfigEntry(JsonSerializer.Serialize(config), true);
        }

        public T? TryGet<T>(string key)
        {
            try
            {
                if (_store.TryGetValue(key, out var config))
                {
                    return JsonSerializer.Deserialize<T>(config.Value);
                }
            }
            catch (Exception)
            {
            }

            return default;
        }

        public T Get<T>(string key)
        {
            return JsonSerializer.Deserialize<T>(_store[key].Value)
                ?? throw new NullReferenceException($"Config value for key '{key}' is null.");
        }

        private void Load()
        {
            var files = Directory.EnumerateFiles(_configFolder, "*.json");

            foreach (var file in files)
            {
                var key = Path.GetFileNameWithoutExtension(file);
                var config = File.ReadAllText(file);
                _store[key] = new ConfigEntry(config, false);
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
