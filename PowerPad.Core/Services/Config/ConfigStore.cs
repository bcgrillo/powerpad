using System.Text.Json;
using System.Text.Json.Serialization;

namespace PowerPad.Core.Services.Config
{
    public interface IConfigStore
    {
        void Set<T>(Enum key, T config);

        T Get<T>(Enum key);

        T? TryGet<T>(Enum key);
    }

    internal record ConfigEntry
    {
        private string? _serializedValue;

        public object? Value { get; private set; }
        public bool Dirty { get; set; }

        public ConfigEntry(string serializedValue, bool dirty)
        {
            _serializedValue = serializedValue;
            Dirty = dirty;
            Value = null;
        }

        public ConfigEntry(object? value, bool dirty)
        {
            _serializedValue = null;
            Dirty = dirty;
            Value = value;
        }

        public T? GetValue<T>(JsonSerializerContext context)
        {
            if (Value is null)
            {
                Value = _serializedValue is not null
                ? (T?)JsonSerializer.Deserialize(_serializedValue, typeof(T), context)
                : null;

                _serializedValue = null;
            }

            return (T?)Value;
        }
    }

    public class ConfigStore : IConfigStore
    {
        private readonly string _configFolder;
        private readonly Dictionary<string, ConfigEntry> _store;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly JsonSerializerContext _context;

        public ConfigStore(string configFolder, JsonSerializerContext context)
        {
            if (!Directory.Exists(configFolder)) Directory.CreateDirectory(configFolder);

            _configFolder = configFolder;
            _context = context;
            _store = [];

            Load();
        }

        public void Set<T>(Enum key, T config)
        {
            _store[key.ToString()] = new(value: config, true);
        }

        public T? TryGet<T>(Enum key)
        {
            try
            {
                if (_store.TryGetValue(key.ToString(), out var config))
                {
                    return _store[key.ToString()].GetValue<T>(_context);
                }
            }
            catch { } //It's ok

            return default;
        }

        public T Get<T>(Enum key)
        {
            return _store[key.ToString()].GetValue<T>(_context)
                ?? throw new NullReferenceException($"Config value for key '{key}' is not found.");
        }

        private void Load()
        {
            var files = Directory.EnumerateFiles(_configFolder, "*.json");

            foreach (var file in files)
            {
                var key = Path.GetFileNameWithoutExtension(file);
                var config = File.ReadAllText(file);
                _store[key] = new(serializedValue: config, false);
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
                        await File.WriteAllTextAsync
                        (
                            path, 
                            JsonSerializer.Serialize(value.Value, value.Value?.GetType() ?? typeof(object), _context)
                        );
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
