using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using PowerPad.Core.Models;
using PowerPad.Core.Services;
using PowerPad.WinUI.Configuration;
using PowerPad.WinUI.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PowerPad.WinUI.Configuration.ConfigConstants;

namespace PowerPad.WinUI.ViewModels
{
    public partial class SettingsViewModel : ObservableObject, IRecipient<AIModelChanged>
    {
        private readonly IConfigStore _configStore;
        private readonly IOllamaService _ollama;
        private readonly IAzureAIService _azureAI;
        private readonly IOpenAIService _openAI;

        public GeneralSettings General { get; private set; }

        public ModelsSettings Models { get; private set; }

        [ObservableProperty]
        private OllamaStatus _ollamaStatus;

        public SettingsViewModel()
        {
            _configStore = App.Get<IConfigStore>();
            _ollama = App.Get<IOllamaService>();
            _azureAI = App.Get<IAzureAIService>();
            _openAI = App.Get<IOpenAIService>();

            General = _configStore.TryGet<GeneralSettings>(StoreKey.GeneralSettings) ?? StoreDefault.GeneralSettings;
            Models = _configStore.TryGet<ModelsSettings>(StoreKey.ModelsSettings) ?? StoreDefault.ModelsSettings;

            _ = Task.Run(async() =>
            {
                OllamaStatus = await _ollama.GetStatus();
            });

            General.PropertyChanged += (s, o) => SaveGeneralSettings();
            Models.PropertyChanged += (s, o) => SaveModelsSettings();

            WeakReferenceMessenger.Default.Register(this);
        }

        public void Receive(AIModelChanged message) => SaveModelsSettings();

        private void SaveGeneralSettings() => _configStore.Set(StoreKey.GeneralSettings, General);

        private void SaveModelsSettings() => _configStore.Set(StoreKey.ModelsSettings, Models);
    }
}
