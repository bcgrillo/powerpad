using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
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
        private AIServiceConfigViewModel? _azureAIConfig;

        [ObservableProperty]
        private AIServiceConfigViewModel? _openAIConfig;

        [ObservableProperty]
        private ApplicationTheme? _appTheme;

        [ObservableProperty]
        private bool _acrylicBackground;

        public GeneralSettingsViewModel()
        {
            PropertyChanged += PropertyChangedHandler;
        }

        private void PropertyChangedHandler(object? _, PropertyChangedEventArgs eventArgs)
        {
            switch(eventArgs.PropertyName)
            {
                case nameof(OllamaConfig):
                    if (OllamaConfig != null)
                        OllamaConfig.PropertyChanged += (s, e) => OnPropertyChanged($"{nameof(OllamaConfig)}.{e.PropertyName}");
                    break;
                case nameof(AzureAIConfig):
                    if (AzureAIConfig != null)
                        AzureAIConfig.PropertyChanged += (s, e) => OnPropertyChanged($"{nameof(AzureAIConfig)}.{e.PropertyName}");
                    break;
                case nameof(OpenAIConfig):
                    if (OpenAIConfig != null)
                        OpenAIConfig.PropertyChanged += (s, e) => OnPropertyChanged($"{nameof(OpenAIConfig)}.{e.PropertyName}");
                    break;
            }
        }
    };
}