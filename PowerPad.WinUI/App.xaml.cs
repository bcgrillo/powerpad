using System;
using Microsoft.UI.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using PowerPad.WinUI.Configuration;
using PowerPad.WinUI.Helpers;
using PowerPad.WinUI.ViewModels.Settings;
using PowerPad.Core.Services.Config;
using PowerPad.Core.Services.FileSystem;
using PowerPad.WinUI.ViewModels.FileSystem;
using PowerPad.WinUI.ViewModels.Agents;
using Microsoft.UI.Dispatching;

namespace PowerPad.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private static IServiceProvider _serviceProvider = null!;
        private readonly IConfigStore _appConfigStore;

        public IServiceProvider ServiceProvider => _serviceProvider;
        public IConfigStore AppConfigStore => _appConfigStore;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            _serviceProvider = new ServiceCollection()
                .ConfigureWorkspaceService(this)
                .ConfigureOllamaService(this)
                .ConfigureAzureAIService(this)
                .ConfigureOpenAIService(this)
                .ConfigureAIService()
                .AddSingleton<IConfigStoreService, ConfigStoreService>()
                .AddSingleton<IDocumentService, DocumentService>()
                .AddSingleton<IOrderService, OrderService>()
                .AddSingleton<SettingsViewModel>()
                .AddSingleton<WorkspaceViewModel>()
                .AddSingleton<AgentsCollectionViewModel>()
                .AddSingleton(_ => AppConfigStore)
                .BuildServiceProvider();

            //Remove?
            //Ioc.Default.ConfigureServices(_serviceProvider);

            this.InitializeAppConfigStore(out _appConfigStore);

            var appTheme = Get<SettingsViewModel>().General.AppTheme;
            if (appTheme is not null) Current.RequestedTheme = appTheme.Value;
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs _)
        {
            m_window = WindowHelper.CreateWindow<MainWindow>();
            m_window.Activate();
        }

        private Window? m_window;

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