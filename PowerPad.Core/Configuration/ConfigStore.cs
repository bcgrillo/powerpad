using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PowerPad.Core.Configuration
{
    public interface IConfigStore
    {
        void SaveConfig<T>(string key, T config);
        T? GetConfig<T>(string key);
    }

    internal struct ConfigEntry
    {
        public string Value { get; set; }
        public bool Dirty { get; set; }

        public ConfigEntry(string value, bool dirty)
        {
            Value = value;
            Dirty = dirty;
        }
    }

    public class ConfigStore : IConfigStore
    {
        private string _configFolder;
        private Dictionary<string, ConfigEntry> _store;

        public ConfigStore(string configFolder)
        {
            if (!Directory.Exists(configFolder)) Directory.CreateDirectory(configFolder);

            _configFolder = configFolder;
            _store = [];

            LoadConfig();
        }

        public void SaveConfig<T>(string key, T config)
        {
            _store[key] = new ConfigEntry(JsonSerializer.Serialize(config), true);
        }

        public T? GetConfig<T>(string key)
        {
            if (_store.TryGetValue(key, out var config))
            {
                return JsonSerializer.Deserialize<T>(config.Value);
            }
            return default;
        }

        private void LoadConfig()
        {
            var files = Directory.EnumerateFiles(_configFolder, "*.json");

            foreach (var file in files)
            {
                var key = Path.GetFileNameWithoutExtension(file);
                var config = File.ReadAllText(file);
                _store[key] = new ConfigEntry(config, false);
            };
        }

        internal async Task StoreConfig()
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
    }
}
