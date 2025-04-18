using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;
using PowerPad.WinUI.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace PowerPad.WinUI.ViewModels.Settings
{
    public partial class GeneralSettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _ollamaEnabled;

        [ObservableProperty]
        private bool _azureAIEnabled;

        [ObservableProperty]
        private bool _openAIEnabled;

        [ObservableProperty]
        private bool _ollamaAutostart;

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

        public required ObservableCollection<ModelProvider> AvailableProviders
        {
            get;
            init
            {
                field = value;
                field.CollectionChanged += AvailableProvidersCollectionChangedHandler;
            }
        }

        [ObservableProperty]
        private ApplicationTheme? _appTheme;

        [ObservableProperty]
        private bool _acrylicBackground;

        [ObservableProperty]
        private string? _agentPrompt;

        public event EventHandler? ProviderAvaibilityChanged;
        public event EventHandler? ServiceEnablementChanged;

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

        private void ServiceConfigChanged(object? sender, EventArgs __)
        {
            var config = (AIServiceConfigViewModel)sender!;

            if (config == OllamaConfig) SetServiceConfig(OllamaEnabled, config, ModelProvider.Ollama, false);
            else if (config == AzureAIConfig) SetServiceConfig(AzureAIEnabled, config, ModelProvider.GitHub, true);
            else if (config == OpenAIConfig) SetServiceConfig(OpenAIEnabled, config, ModelProvider.OpenAI, true);
        }

        partial void OnOllamaEnabledChanged(bool value)
        {
            SetServiceConfig(value, OllamaConfig, ModelProvider.Ollama, false);
            ServiceEnablementChanged?.Invoke(OllamaConfig, EventArgs.Empty);
        }

        partial void OnAzureAIEnabledChanged(bool value)
        {
            SetServiceConfig(value, AzureAIConfig, ModelProvider.GitHub, true);
            ServiceEnablementChanged?.Invoke(AzureAIConfig, EventArgs.Empty);
        }

        partial void OnOpenAIEnabledChanged(bool value)
        {
            SetServiceConfig(value, OpenAIConfig, ModelProvider.OpenAI, true);
            ServiceEnablementChanged?.Invoke(OpenAIConfig, EventArgs.Empty);
        }

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

        private void AvailableProvidersCollectionChangedHandler(object? _, NotifyCollectionChangedEventArgs eventArgs)
        {
            ProviderAvaibilityChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged(nameof(AvailableProviders));
        }

        partial void OnAcrylicBackgroundChanging(bool value)
        {
            WindowHelper.MainWindow?.SetBackdrop(value);
        }
    };
}