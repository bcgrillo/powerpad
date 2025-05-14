using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;
using PowerPad.WinUI.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace PowerPad.WinUI.ViewModels.Settings
{
    /// <summary>
    /// ViewModel for managing general settings of the application, including AI services and UI preferences.
    /// </summary>
    public partial class GeneralSettingsViewModel : ObservableObject
    {
        /// <summary>
        /// Gets or sets a value indicating whether the Ollama AI service is enabled.
        /// </summary>
        [ObservableProperty]
        public partial bool OllamaEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Azure AI service is enabled.
        /// </summary>
        [ObservableProperty]
        public partial bool AzureAIEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the OpenAI service is enabled.
        /// </summary>
        [ObservableProperty]
        public partial bool OpenAIEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Ollama AI service should start automatically.
        /// </summary>
        [ObservableProperty]
        public partial bool OllamaAutostart { get; set; }

        /// <summary>
        /// Gets the configuration for the Ollama AI service.
        /// </summary>
        public required AIServiceConfigViewModel OllamaConfig
        {
            get;
            init
            {
                field = value;
                field.StatusChanged += ServiceStatusChanged;
                field.ConfigChanged += ServiceConfigChanged;
            }
        }

        /// <summary>
        /// Gets the configuration for the Azure AI service.
        /// </summary>
        public required AIServiceConfigViewModel AzureAIConfig
        {
            get;
            init
            {
                field = value;
                field.StatusChanged += ServiceStatusChanged;
                field.ConfigChanged += ServiceConfigChanged;
            }
        }

        /// <summary>
        /// Gets the configuration for the OpenAI service.
        /// </summary>
        public required AIServiceConfigViewModel OpenAIConfig
        {
            get;
            init
            {
                field = value;
                field.StatusChanged += ServiceStatusChanged;
                field.ConfigChanged += ServiceConfigChanged;
            }
        }

        /// <summary>
        /// Gets the collection of available AI model providers.
        /// </summary>
        public required ObservableCollection<ModelProvider> AvailableProviders
        {
            get;
            init
            {
                field = value ?? [];
                field.CollectionChanged += AvailableProvidersCollectionChangedHandler;
            }
        }

        /// <summary>
        /// Gets or sets the application theme.
        /// </summary>
        [ObservableProperty]
        public partial ApplicationTheme? AppTheme { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the acrylic background effect is enabled.
        /// </summary>
        [ObservableProperty]
        public partial bool AcrylicBackground { get; set; }

        /// <summary>
        /// Gets or sets the agent prompt text.
        /// </summary>
        [ObservableProperty]
        public partial string? AgentPrompt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether hotkeys are enabled.
        /// </summary>
        [ObservableProperty]
        public partial bool EnableHotKeys { get; set; }

        /// <summary>
        /// Event triggered when the availability of providers changes.
        /// </summary>
        public event EventHandler? ProviderAvailabilityChanged;

        /// <summary>
        /// Event triggered when the enablement of a service changes.
        /// </summary>
        public event EventHandler? ServiceEnablementChanged;

        /// <summary>
        /// Initializes the AI services based on their configurations.
        /// </summary>
        public void InitializeAIServices()
        {
            SetServiceConfig(OllamaEnabled, OllamaConfig, ModelProvider.Ollama, false);
            SetServiceConfig(AzureAIEnabled, AzureAIConfig, ModelProvider.GitHub, false);
            SetServiceConfig(OpenAIEnabled, OpenAIConfig, ModelProvider.OpenAI, false);
        }

        /// <summary>
        /// Handles changes to the AcrylicBackground property.
        /// </summary>
        /// <param name="value">The new value of the property.</param>
        partial void OnAcrylicBackgroundChanging(bool value)
        {
            App.MainWindow?.SetBackdrop(value);
        }

        /// <summary>
        /// Handles changes to the EnableHotKeys property.
        /// </summary>
        /// <param name="value">The new value of the property.</param>
        partial void OnEnableHotKeysChanging(bool value)
        {
            if (App.MainWindow is not null)
                HotKeyHelper.Register(App.MainWindow, value);
        }

        /// <summary>
        /// Handles changes to the OllamaEnabled property.
        /// </summary>
        /// <param name="value">The new value of the property.</param>
        partial void OnOllamaEnabledChanged(bool value)
        {
            if (OllamaConfig is not null) // Avoid running when GeneralSettingsViewModel is not fully initialized
            {
                SetServiceConfig(value, OllamaConfig, ModelProvider.Ollama, false);
                ServiceEnablementChanged?.Invoke(OllamaConfig, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handles changes to the AzureAIEnabled property.
        /// </summary>
        /// <param name="value">The new value of the property.</param>
        partial void OnAzureAIEnabledChanged(bool value)
        {
            if (AzureAIConfig is not null) // Avoid running when GeneralSettingsViewModel is not fully initialized
            {
                SetServiceConfig(value, AzureAIConfig, ModelProvider.GitHub, true);
                ServiceEnablementChanged?.Invoke(AzureAIConfig, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handles changes to the OpenAIEnabled property.
        /// </summary>
        /// <param name="value">The new value of the property.</param>
        partial void OnOpenAIEnabledChanged(bool value)
        {
            if (OpenAIConfig is not null) // Avoid running when GeneralSettingsViewModel is not fully initialized
            {
                SetServiceConfig(value, OpenAIConfig, ModelProvider.OpenAI, true);
                ServiceEnablementChanged?.Invoke(OpenAIConfig, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handles changes in the status of an AI service.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="__">Event arguments (not used).</param>
        private void ServiceStatusChanged(object? sender, EventArgs __)
        {
            var config = (AIServiceConfigViewModel)sender!;

            if (config == OllamaConfig)
            {
                if (config.ServiceStatus == ServiceStatus.Online)
                {
                    if (!AvailableProviders.Contains(ModelProvider.Ollama)) AvailableProviders.Add(ModelProvider.Ollama);
                    if (!AvailableProviders.Contains(ModelProvider.HuggingFace)) AvailableProviders.Add(ModelProvider.HuggingFace);
                }
                else
                {
                    AvailableProviders.Remove(ModelProvider.Ollama);
                    AvailableProviders.Remove(ModelProvider.HuggingFace);
                }
            }
            else if (config == AzureAIConfig)
            {
                if (config.ServiceStatus == ServiceStatus.Online)
                {
                    if (!AvailableProviders.Contains(ModelProvider.GitHub)) AvailableProviders.Add(ModelProvider.GitHub);
                }
                else AvailableProviders.Remove(ModelProvider.GitHub);
            }
            else if (config == OpenAIConfig)
            {
                if (config.ServiceStatus == ServiceStatus.Online)
                {
                    if (!AvailableProviders.Contains(ModelProvider.OpenAI)) AvailableProviders.Add(ModelProvider.OpenAI);
                }
                else AvailableProviders.Remove(ModelProvider.OpenAI);
            }
        }

        /// <summary>
        /// Handles changes in the configuration of an AI service.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="__">Event arguments (not used).</param>
        private void ServiceConfigChanged(object? sender, EventArgs __)
        {
            var config = (AIServiceConfigViewModel)sender!;

            if (config == OllamaConfig) SetServiceConfig(OllamaEnabled, config, ModelProvider.Ollama, false);
            else if (config == AzureAIConfig) SetServiceConfig(AzureAIEnabled, config, ModelProvider.GitHub, true);
            else if (config == OpenAIConfig) SetServiceConfig(OpenAIEnabled, config, ModelProvider.OpenAI, true);
        }

        /// <summary>
        /// Configures an AI service based on its enablement status and configuration.
        /// </summary>
        /// <param name="enabled">Indicates whether the service is enabled.</param>
        /// <param name="config">The configuration of the AI service.</param>
        /// <param name="modelProvider">The provider of the AI model.</param>
        /// <param name="keyIsRequired">Indicates whether an API key is required for the service.</param>
        private static void SetServiceConfig(bool enabled, AIServiceConfigViewModel config, ModelProvider modelProvider, bool keyIsRequired)
        {
            if (enabled && !string.IsNullOrEmpty(config.BaseUrl) && (!keyIsRequired || !string.IsNullOrEmpty(config.Key)))
            {
                var aiService = App.Get<IAIService>(modelProvider);
                aiService.Initialize(config.GetRecord());
            }
            else
            {
                var aiService = App.Get<IAIService>(modelProvider);
                aiService.Initialize(null);
                config.ResetStatus();
            }
        }

        /// <summary>
        /// Handles changes to the collection of available providers.
        /// </summary>
        /// <param name="_">The source of the event (not used).</param>
        /// <param name="eventArgs">The event arguments containing details of the change.</param>
        private void AvailableProvidersCollectionChangedHandler(object? _, NotifyCollectionChangedEventArgs eventArgs)
        {
            ProviderAvailabilityChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged(nameof(AvailableProviders));
        }
    }
}