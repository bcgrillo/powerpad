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
using PowerPad.Core.Configuration;
using PowerPad.WinUI.Configuration;
using PowerPad.WinUI.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PowerPad.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;
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
                .ConfigureAIService(this)
                .AddSingleton<IConfigStoreService, ConfigStoreService>()
                .AddSingleton<IDocumentService, DocumentService>()
                .AddSingleton<IOrderService, OrderService>()
                .AddSingleton<AIServicesVMCollection>()
                .AddSingleton(_ => AppConfigStore)
                .BuildServiceProvider();

            Ioc.Default.ConfigureServices(_serviceProvider);

            _appConfigStore = this.InitializeAppConfigStore();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = WindowHelper.CreateWindow<MainWindow>();
            m_window.Activate();
        }

        private Window? m_window;
    }
}
