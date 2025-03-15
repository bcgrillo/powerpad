using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using PowerPad.Core.Configuration;
using PowerPad.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static PowerPad.WinUI.Configuration.ConfigConstants;

namespace PowerPad.WinUI.Configuration
{
    public static class AppConfiguration
    {
        public static IServiceCollection ConfigureWorkspaceService(this IServiceCollection serviceColection, App app)
        {
            return serviceColection.AddSingleton<IWorkspaceService, WorkspaceService>(sp =>
            {
                var workspaceFolder = GetWorkspacePath(app);
                var configStoreService = sp.GetRequiredService<IConfigStoreService>();

                return new WorkspaceService(workspaceFolder, configStoreService);
            });
        }

        public static IServiceCollection ConfigureOllamaService(this IServiceCollection serviceColection, App app)
        {
            return serviceColection.AddSingleton<IOllamaService, OllamaService>(sp =>
            {
                string ollamaServiceUrl = GetOllamaServiceUrl(app.AppConfigStore);

                return new OllamaService(ollamaServiceUrl);
            });
        }

        public static IConfigStore InitializeAppConfigStore(this App app)
        {
            var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $".{nameof(PowerPad).ToLower()}");
            var configStoreService = app.ServiceProvider.GetRequiredService<IConfigStoreService>();

            return configStoreService.GetConfigStore(appDataFolder);
        }

        private static string GetWorkspacePath(App app)
        {
            var workspaceFolder = app.AppConfigStore.GetConfig<string>(Keys.WorkspaceFolder);

            if (workspaceFolder == null)
            {
                workspaceFolder = Defaults.WorkspaceFolder;
                app.AppConfigStore.SaveConfig(Keys.WorkspaceFolder, workspaceFolder);
            }

            return workspaceFolder;
        }

        private static string GetOllamaServiceUrl(IConfigStore config)
        {
            var configUrl = config.GetConfig<string>(Keys.OllamaServiceUrl);

            if (configUrl == null)
            {
                configUrl = Defaults.OllamaServiceUrl;
                config.SaveConfig(Keys.OllamaServiceUrl, configUrl);
            };

            return configUrl;
        }
    }
}