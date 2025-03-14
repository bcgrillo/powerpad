using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using CommunityToolkit.Mvvm.DependencyInjection;
using PowerPad.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using PowerPad.WinUI.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PowerPad.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            string workspacePath = GetOllamaServiceUrl();
            string ollamaServiceUrl = GetOllamaServiceUrl();

            Ioc.Default.ConfigureServices(
                new ServiceCollection()
                    .AddSingleton<IConfigStoreService, ConfigStoreService>()
                    .AddSingleton<IWorkspaceService, WorkspaceService>(_ =>
                    {
                        var configStoreService = Ioc.Default.GetRequiredService<IConfigStoreService>();
                        return new WorkspaceService(workspacePath, configStoreService);
                    })
                    .AddSingleton<IDocumentService, DocumentService>()
                    .AddTransient<WorkspaceViewModel>()
                    .AddSingleton<IOllamaService>(_ => new OllamaService(ollamaServiceUrl))
                    .BuildServiceProvider()
            );
        }

        private string GetOllamaServiceUrl()
        {
            // Logic to check if a configuration exists
            // If not, return a default URL
            string? configUrl = LoadConfigurationUrl();
            return configUrl ?? "http://localhost";
        }

        private string? LoadConfigurationUrl()
        {
            // Implement the logic to load the configuration URL
            // Return null if no configuration is found
            return null; // Placeholder for actual implementation
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }

        private Window? m_window;
    }
}
