using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PowerPad.Core.Config
{
    public interface IConfigStore
    {
        void SaveConfig<T>(string key, T config);
        T? GetConfig<T>(string key);
        Task StoreConfig();
    }

    public class ConfigStore : IConfigStore
    {
        private string _configFolder;
        private Dictionary<string, (object? value, bool dirty)> _configStore = [];

        public ConfigStore(string configFolder)
        {
            _configFolder = configFolder;
        }

        public void SaveConfig<T>(string key, T config)
        {
            _configStore[key] = (config, true);
        }

        public T? GetConfig<T>(string key)
        {
            if (_configStore.TryGetValue(key, out var config))
            {
                return (T?)config.value;
            }
            return default;
        }

        public async Task StoreConfig()
        {
            for (var i = 0; i < _configStore.Count; i++)
            {
                var (key, value) = _configStore.ElementAt(i);
                if (value.dirty)
                {
                    var path = Path.Combine(_configFolder, $"{key}.json");
                    var jsonConfig = JsonSerializer.Serialize(value.value);
                    await File.WriteAllTextAsync(path, jsonConfig);
                    value.dirty = false;
                }
            }
        }
    }
}
