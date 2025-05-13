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
    /// Represents the main application class responsible for initializing services and the main window.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// The service provider for dependency injection.
        /// </summary>
        private static IServiceProvider _serviceProvider = null!;

        /// <summary>
        /// The application configuration store.
        /// </summary>
        private static IConfigStore _appConfigStore = null!;

        /// <summary>
        /// The main application window.
        /// </summary>
        private static MainWindow _window = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            InitializeServiceCollection();

            var appTheme = Get<SettingsViewModel>().General.AppTheme;
            if (appTheme is not null) Current.RequestedTheme = appTheme.Value;
        }

        /// <summary>
        /// Handles the application launch event and initializes the main window.
        /// </summary>
        /// <param name="args">The launch activation arguments.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            InitializeMainWindow();
        }

        /// <summary>
        /// Initializes the service collection and configures dependency injection.
        /// </summary>
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

        /// <summary>
        /// Initializes and activates the main application window.
        /// </summary>
        private static void InitializeMainWindow()
        {
            _window = new();
            _window.Activate();
        }

        /// <summary>
        /// Gets the main application window.
        /// </summary>
        public static MainWindow? MainWindow => _window;

        /// <summary>
        /// Retrieves a service of the specified type from the service provider.
        /// </summary>
        /// <typeparam name="T">The type of the service to retrieve.</typeparam>
        /// <param name="key">An optional key for keyed services.</param>
        /// <returns>The requested service instance.</returns>
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