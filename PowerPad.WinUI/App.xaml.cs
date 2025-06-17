using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using PowerPad.Core.Services.Config;
using PowerPad.Core.Services.FileSystem;
using PowerPad.WinUI.Configuration;
using PowerPad.WinUI.ViewModels.Agents;
using PowerPad.WinUI.ViewModels.FileSystem;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Storage;
using WinUI3Localizer;

namespace PowerPad.WinUI
{
    /// <summary>
    /// Represents the main application class responsible for initializing services and the main window.
    /// </summary>
    public partial class App : Application
    {
        private static IServiceProvider _serviceProvider = null!;
        private static IConfigStore _appConfigStore = null!;
        private static MainWindow _window = null!;

        private ILocalizer Localizer { get; set; } = WinUI3Localizer.Localizer.Get();

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
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            await InitializeLocalizer();

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

        /// <summary>
        /// Initializes the localizer by creating necessary string resource files and setting up localization options.
        /// </summary>
        private async Task InitializeLocalizer()
        {
            // Initialize a "Strings" folder in the "LocalFolder" for the packaged app.
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFolder stringsFolder = await localFolder.CreateFolderAsync(
              "Strings",
               CreationCollisionOption.OpenIfExists);

            // Create string resources file from app resources if doesn't exist.
            string resourceFileName = "Resources.resw";
            await CreateStringResourceFileIfNotExists(stringsFolder, "es-ES", resourceFileName);
            await CreateStringResourceFileIfNotExists(stringsFolder, "en-GB", resourceFileName);

            await new LocalizerBuilder()
                .AddStringResourcesFolderForLanguageDictionaries(stringsFolder.Path)
                .SetOptions(options =>
                {
                    options.DefaultLanguage = "en-GB";
                })
                .Build();
        }

        /// <summary>
        /// Creates a string resource file in the specified language folder if it does not already exist.
        /// </summary>
        /// <param name="stringsFolder">The parent folder containing language-specific folders.</param>
        /// <param name="language">The language code for the folder (e.g., "es-ES").</param>
        /// <param name="resourceFileName">The name of the resource file to create.</param>
        private static async Task CreateStringResourceFileIfNotExists(StorageFolder stringsFolder, string language, string resourceFileName)
        {
            StorageFolder languageFolder = await stringsFolder.CreateFolderAsync(
                language,
                CreationCollisionOption.OpenIfExists);

            //if (await languageFolder.TryGetItemAsync(resourceFileName) is null)
            //{
                string resourceFilePath = Path.Combine(stringsFolder.Name, language, resourceFileName);
                StorageFile resourceFile = await LoadStringResourcesFileFromAppResource(resourceFilePath);
                _ = await resourceFile.CopyAsync(languageFolder, resourceFileName, NameCollisionOption.ReplaceExisting);
            //}
        }

        /// <summary>
        /// Loads a string resource file from the application's packaged resources.
        /// </summary>
        /// <param name="filePath">The relative file path within the application package.</param>
        /// <returns>A <see cref="StorageFile"/> representing the loaded resource file.</returns>
        private static async Task<StorageFile> LoadStringResourcesFileFromAppResource(string filePath)
        {
            Uri resourcesFileUri = new($"ms-appx:///{filePath}");
            return await StorageFile.GetFileFromApplicationUriAsync(resourcesFileUri);
        }
    }
}