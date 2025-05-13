using System.Text.Json.Serialization;
using Timer = System.Timers.Timer;

namespace PowerPad.Core.Services.Config
{
    /// <summary>
    /// Provides methods for managing configuration stores and saving configurations.
    /// </summary>
    public interface IConfigStoreService
    {
        /// <summary>
        /// Retrieves or creates a configuration store for the specified folder.
        /// </summary>
        /// <param name="configFolder">The folder where the configuration store is located.</param>
        /// <returns>An instance of <see cref="IConfigStore"/>.</returns>
        IConfigStore GetConfigStore(string configFolder);

        /// <summary>
        /// Saves all configurations to their respective files.
        /// </summary>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        Task StoreConfigs();
    }

    /// <summary>
    /// Implementation of <see cref="IConfigStoreService"/> that manages configuration stores and periodic saving.
    /// </summary>
    public class ConfigStoreService : IConfigStoreService
    {
        private const double STORE_INTERVAL = 2000;

        private readonly JsonSerializerContext _context;
        private readonly Dictionary<string, ConfigStore> _configStores;

#pragma warning disable S1450 // Private field _timer required for keep the timer instance
        private readonly Timer _timer;
#pragma warning restore S1450

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigStoreService"/> class.
        /// </summary>
        /// <param name="context">The JSON serializer context used for serialization.</param>
        public ConfigStoreService(JsonSerializerContext context)
        {
            _context = context;
            _configStores = [];

            _timer = new(STORE_INTERVAL);
            _timer.Elapsed += async (s, e) => await StoreConfigs();
            _timer.Start();

            AppDomain.CurrentDomain.ProcessExit += async (s, e) => await StoreConfigs();
        }

        /// <inheritdoc />
        public IConfigStore GetConfigStore(string configFolder)
        {
            if (_configStores.TryGetValue(configFolder, out var configStore))
            {
                return configStore;
            }
            else
            {
                var newConfigStore = new ConfigStore(configFolder, _context);
                _configStores[configFolder] = newConfigStore;
                return newConfigStore;
            }
        }

        /// <inheritdoc />
        public async Task StoreConfigs()
        {
            var tasks = _configStores.Values.Select(cs => cs.Save());
            await Task.WhenAll(tasks);
        }
    }
}