using Timer = System.Timers.Timer;

namespace PowerPad.Core.Services.Config
{
    public interface IConfigStoreService
    {
        IConfigStore GetConfigStore(string configFolder);
    }

    public class ConfigStoreService : IConfigStoreService
    {
        private const double STORE_INTERVAL = 2000;

        private readonly Timer _timer;

        private readonly Dictionary<string, ConfigStore> _configStores = [];

        public ConfigStoreService()
        {
            AppDomain.CurrentDomain.ProcessExit += async (s, e) => await StoreConfigs();

            _timer = new(STORE_INTERVAL);
            _timer.Elapsed += async (s, e) => await StoreConfigs();
            _timer.Start();
        }

        public IConfigStore GetConfigStore(string configFolder)
        {
            if (_configStores.TryGetValue(configFolder, out var configStore))
            {
                return configStore;
            }
            else
            {
                var newConfigStore = new ConfigStore(configFolder);
                _configStores[configFolder] = newConfigStore;
                return newConfigStore;
            }
        }

        private async Task StoreConfigs()
        {
            var tasks = _configStores.Values.Select(cs => cs.Save());
            await Task.WhenAll(tasks);
        }
    }
}