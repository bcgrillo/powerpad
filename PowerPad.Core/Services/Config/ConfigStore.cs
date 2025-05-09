using System.Text.Json;
using System.Text.Json.Serialization;

namespace PowerPad.Core.Services.Config
{
    /// <summary>
    /// Defines methods for storing and retrieving configuration values.
    /// </summary>
    public interface IConfigStore
    {
        /// <summary>
        /// Sets a configuration value for the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the configuration value.</typeparam>
        /// <param name="key">The key associated with the configuration value.</param>
        /// <param name="config">The configuration value to store.</param>
        void Set<T>(Enum key, T config);

        /// <summary>
        /// Retrieves a configuration value for the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the configuration value.</typeparam>
        /// <param name="key">The key associated with the configuration value.</param>
        /// <returns>The configuration value.</returns>
        T Get<T>(Enum key);

        /// <summary>
        /// Attempts to retrieve a configuration value for the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the configuration value.</typeparam>
        /// <param name="key">The key associated with the configuration value.</param>
        /// <returns>The configuration value if found; otherwise, <c>null</c>.</returns>
        T? TryGet<T>(Enum key);
    }

    /// <summary>
    /// Represents a configuration entry with its serialized value and state.
    /// </summary>
    internal record ConfigEntry
    {
        private string? _serializedValue;

        /// <summary>
        /// Gets the deserialized value of the configuration entry.
        /// </summary>
        public object? Value { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the configuration entry has been modified.
        /// </summary>
        public bool Dirty { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigEntry"/> class with a serialized value.
        /// </summary>
        /// <param name="serializedValue">The serialized value of the configuration entry.</param>
        /// <param name="dirty">Indicates whether the entry is dirty.</param>
        public ConfigEntry(string serializedValue, bool dirty)
        {
            _serializedValue = serializedValue;
            Dirty = dirty;
            Value = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigEntry"/> class with a deserialized value.
        /// </summary>
        /// <param name="value">The deserialized value of the configuration entry.</param>
        /// <param name="dirty">Indicates whether the entry is dirty.</param>
        public ConfigEntry(object? value, bool dirty)
        {
            _serializedValue = null;
            Dirty = dirty;
            Value = value;
        }

        /// <summary>
        /// Gets the value of the configuration entry, deserializing it if necessary.
        /// </summary>
        /// <typeparam name="T">The type of the configuration value.</typeparam>
        /// <param name="context">The JSON serializer context.</param>
        /// <returns>The deserialized configuration value.</returns>
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

    /// <summary>
    /// Provides functionality for managing configuration storage and retrieval.
    /// </summary>
    public class ConfigStore : IConfigStore
    {
        private readonly string _configFolder;
        private readonly Dictionary<string, ConfigEntry> _store;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly JsonSerializerContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigStore"/> class.
        /// </summary>
        /// <param name="configFolder">The folder where configuration files are stored.</param>
        /// <param name="context">The JSON serializer context.</param>
        public ConfigStore(string configFolder, JsonSerializerContext context)
        {
            if (!Directory.Exists(configFolder)) Directory.CreateDirectory(configFolder);

            _configFolder = configFolder;
            _context = context;
            _store = [];

            Load();
        }

        /// <inheritdoc />
        public void Set<T>(Enum key, T config)
        {
            _store[key.ToString()] = new(value: config, true);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public T Get<T>(Enum key)
        {
            return _store[key.ToString()].GetValue<T>(_context)
                ?? throw new NullReferenceException($"Config value for key '{key}' is not found.");
        }

        /// <summary>
        /// Loads configuration entries from the configuration folder.
        /// </summary>
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

        /// <summary>
        /// Saves all dirty configuration entries to their respective files.
        /// </summary>
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