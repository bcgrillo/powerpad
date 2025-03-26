using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using PowerPad.Core.Configuration;
using PowerPad.Core.Models;
using PowerPad.Core.Services;
using PowerPad.WinUI.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PowerPad.WinUI.Configuration.ConfigConstants;

namespace PowerPad.WinUI.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly IConfigStore _configStore;
        private readonly IOllamaService _ollama;
        private readonly IAzureAIService _azureAI;
        private readonly IOpenAIService _openAI;
        private GeneralSettings _general;

        [ObservableProperty]
        private OllamaStatus _ollamaStatus;

        public bool OllamaEnabled
        {
            get => _general.OllamaEnabled;
            set
            {
                if (_general.OllamaEnabled != value)
                {
                    _general.OllamaEnabled = value;
                    SaveSettings();
                    OnPropertyChanged(nameof(OllamaEnabled));
                }
            }
        }

        public bool AzureAIEnabled
        {
            get => _general.AzureAIEnabled;
            set
            {
                if (_general.AzureAIEnabled != value)
                {
                    _general.AzureAIEnabled = value;
                    SaveSettings();
                    OnPropertyChanged(nameof(AzureAIEnabled));
                }
            }
        }

        public bool OpenAIEnabled
        {
            get => _general.OpenAIEnabled;
            set
            {
                if (_general.OpenAIEnabled != value)
                {
                    _general.OpenAIEnabled = value;
                    SaveSettings();
                    OnPropertyChanged(nameof(OpenAIEnabled));
                }
            }
        }

        public SettingsViewModel()
        {
            _configStore = App.Get<IConfigStore>();
            _ollama = App.Get<IOllamaService>();
            _azureAI = App.Get<IAzureAIService>();
            _openAI = App.Get<IOpenAIService>();

            _general = _configStore.TryGet<GeneralSettings>(Keys.GeneralSettings) ?? Defaults.GeneralSettings;

            _ = Task.Run(async() =>
            {
                OllamaStatus = await _ollama.GetStatus();
            });
        }

        private void SaveSettings()
        {
            _configStore.Set(Keys.GeneralSettings, _general);
        }
    }
}
