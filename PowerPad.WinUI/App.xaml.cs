using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using PowerPad.Core.Services.Config;
using PowerPad.Core.Services.FileSystem;
using PowerPad.WinUI.Configuration;
using PowerPad.WinUI.ViewModels.Agents;
using PowerPad.WinUI.ViewModels.FileSystem;
using PowerPad.WinUI.ViewModels.Settings;
using System;

namespace PowerPad.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private static IServiceProvider _serviceProvider = null!;
        private static IConfigStore _appConfigStore = null!;
        private static MainWindow _window = null!;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            InitializeServiceCollection();

            var appTheme = Get<SettingsViewModel>().General.AppTheme;
            if (appTheme is not null) Current.RequestedTheme = appTheme.Value;
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            InitializeMainWindow();
        }

        private static void InitializeServiceCollection()
        {
            _serviceProvider = new ServiceCollection()
                .ConfigureWorkspaceService()
                .ConfigureOllamaService()
                .ConfigureAzureAIService()
                .ConfigureOpenAIService()
                .ConfigureAIService()
                .ConfigureConfigStoreService()
                .ConfigureOrderService()
                .AddSingleton<IDocumentService, DocumentService>()
                .AddSingleton<SettingsViewModel>()
                .AddSingleton<WorkspaceViewModel>()
                .AddSingleton<AgentsCollectionViewModel>()
                .AddSingleton(_ => _appConfigStore!)
                .BuildServiceProvider();

            AppConfiguration.InitializeAppConfigStore(out _appConfigStore);
        }

        private static void InitializeMainWindow()
        {
            _window = new();
            _window.Activate();
        }

        public static MainWindow? MainWindow => _window;

        public static T Get<T>(object? key = null) where T : notnull
        {
            if (key is not null)
            {
                return _serviceProvider.GetRequiredKeyedService<T>(key);
            }
            else
            {
                return _serviceProvider.GetRequiredService<T>();
            }
        }
    }
}