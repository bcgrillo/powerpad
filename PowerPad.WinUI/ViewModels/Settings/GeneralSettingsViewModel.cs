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

        [ObservableProperty]
        private AIServiceConfigViewModel? _ollamaConfig;

        [ObservableProperty]
        private bool _ollamaAutostart;

        [ObservableProperty]
        private AIServiceConfigViewModel? _azureAIConfig;

        [ObservableProperty]
        private AIServiceConfigViewModel? _openAIConfig; //TODO: Make readonly

        [ObservableProperty]
        private ApplicationTheme? _appTheme;

        [ObservableProperty]
        private bool _acrylicBackground;

        public GeneralSettingsViewModel()
        {
            PropertyChanged += PropertyChangedHandler;
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

        private void PropertyChangedHandler(object? _, PropertyChangedEventArgs eventArgs)
        {
            switch(eventArgs.PropertyName)
            {
                case nameof(OllamaConfig):
                    if (OllamaConfig is not null)
                        OllamaConfig.PropertyChanged += SecondaryPropertyChangedHandler;
                    break;
                case nameof(AzureAIConfig):
                    if (AzureAIConfig is not null)
                        AzureAIConfig.PropertyChanged += SecondaryPropertyChangedHandler;
                    break;
                case nameof(OpenAIConfig):
                    if (OpenAIConfig is not null)
                        OpenAIConfig.PropertyChanged += SecondaryPropertyChangedHandler;
                    break;
            }
        }

        private void SecondaryPropertyChangedHandler(object? _, PropertyChangedEventArgs __) => OnPropertyChanged();
    };
}