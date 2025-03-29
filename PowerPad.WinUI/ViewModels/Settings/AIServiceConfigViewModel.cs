using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Models.AI;
using System.Text.Json.Serialization;

namespace PowerPad.WinUI.ViewModels.Settings
{
    public class AIServiceConfigViewModel(AIServiceConfig aiServiceConfig) : ObservableObject
    {
        private readonly AIServiceConfig _aiServiceConfig = aiServiceConfig;

        [JsonConstructor]
        public AIServiceConfigViewModel(string? baseUrl, string? key)
            : this(new AIServiceConfig
            {
                BaseUrl = baseUrl,
                Key = key
            })
        {
        }

        public string? BaseUrl
        {
            get => _aiServiceConfig.BaseUrl;
            set => SetProperty(_aiServiceConfig.BaseUrl, value, _aiServiceConfig, (x, y) => x.BaseUrl = y);
        }

        public string? Key
        {
            get => _aiServiceConfig.Key;
            set => SetProperty(_aiServiceConfig.Key, value, _aiServiceConfig, (x, y) => x.Key = y);
        }

        public AIServiceConfig GetModel() => _aiServiceConfig;
    }
}
