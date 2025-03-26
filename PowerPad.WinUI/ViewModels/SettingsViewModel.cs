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

        [ObservableProperty]
        private GeneralSettings _general;

        [ObservableProperty]
        private OllamaStatus _ollamaStatus;

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

        public void OnGeneralSettingsChanged()
        {
            _configStore.Set(Keys.GeneralSettings, General);
        }
    }
}
