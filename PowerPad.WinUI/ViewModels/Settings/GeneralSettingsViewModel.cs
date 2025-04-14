using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using PowerPad.Core.Models.AI;
using System.Collections.Generic;
using System.ComponentModel;

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

        public required AIServiceConfigViewModel OllamaConfig
        { 
            get; 
            init
            {
                field = value;
                field.PropertyChanged += (s, e) => OnPropertyChanged(nameof(OllamaConfig));
            }
        }

        [ObservableProperty]
        private bool _ollamaAutostart;

        public required AIServiceConfigViewModel AzureAIConfig
        {
            get;
            init
            {
                field = value;
                field.PropertyChanged += (s, e) => OnPropertyChanged(nameof(AzureAIConfig));
            }
        }

        public required AIServiceConfigViewModel OpenAIConfig
        {
            get;
            init
            {
                field = value;
                field.PropertyChanged += (s, e) => OnPropertyChanged(nameof(OpenAIConfig));
            }
        }

        [ObservableProperty]
        private ApplicationTheme? _appTheme;

        [ObservableProperty]
        private bool _acrylicBackground;

        [ObservableProperty]
        private string? _agentPrompt;

        public GeneralSettingsViewModel()
        {
        }

        public IEnumerable<ModelProvider> GetAvailableModelProviders()
        {
            List<ModelProvider> providers = [];

            if (OllamaEnabled)
            {
                providers.Add(ModelProvider.Ollama);
                providers.Add(ModelProvider.HuggingFace);
            }
            if (AzureAIEnabled)
            {
                providers.Add(ModelProvider.GitHub);
            }
            if (OpenAIEnabled)
            {
                providers.Add(ModelProvider.OpenAI);
            }

            return providers;
        }

        private void SecondaryPropertyChangedHandler(object? _, PropertyChangedEventArgs e) => OnPropertyChanged();
    };
}