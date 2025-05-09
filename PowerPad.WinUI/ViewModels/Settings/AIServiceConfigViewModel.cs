using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PowerPad.WinUI.ViewModels.Settings
{
    public partial class AIServiceConfigViewModel(AIServiceConfig aiServiceConfig) : ObservableObject
    {
        private readonly AIServiceConfig _aiServiceConfig = aiServiceConfig;

        [JsonConstructor]
        public AIServiceConfigViewModel(string? baseUrl, string? key)
            : this(new()
            {
                BaseUrl = baseUrl,
                Key = key
            })
        {
        }

        public string? BaseUrl
        {
            get => _aiServiceConfig.BaseUrl;
            set
            {
                SetProperty(_aiServiceConfig.BaseUrl, value, _aiServiceConfig, (x, y) => x.BaseUrl = y);
                ConfigChanged?.Invoke(this, new(nameof(BaseUrl)));
            }
        }

        public string? Key
        {
            get => _aiServiceConfig.Key;
            set
            {
                SetProperty(_aiServiceConfig.Key, value, _aiServiceConfig, (x, y) => x.Key = y);
                ConfigChanged?.Invoke(this, new(nameof(Key)));
            }
        }

        [JsonIgnore]
        public ServiceStatus ServiceStatus { get; private set; } = ServiceStatus.Unknown;

        [JsonIgnore]
        public string? ErrorMessage { get; private set; }

        public event EventHandler<PropertyChangedEventArgs>? ConfigChanged;
        public event EventHandler? StatusChanged;

        public AIServiceConfig GetRecord() => _aiServiceConfig;

        public async Task TestConnection(IAIService aiService)
        {
            if (ServiceStatus != ServiceStatus.NotFound) ServiceStatus = ServiceStatus.Updating;
            ErrorMessage = null;

            OnPropertyChanged(nameof(ServiceStatus));
            OnPropertyChanged(nameof(ErrorMessage));

            (ServiceStatus, ErrorMessage) = await aiService.TestConnection();

            StatusChanged?.Invoke(this, EventArgs.Empty);

            OnPropertyChanged(nameof(ServiceStatus));
            OnPropertyChanged(nameof(ErrorMessage));
        }

        public void ResetStatus()
        {
            ServiceStatus = ServiceStatus.Unknown;
            ErrorMessage = null;

            StatusChanged?.Invoke(this, EventArgs.Empty);

            OnPropertyChanged(nameof(ServiceStatus));
            OnPropertyChanged(nameof(ErrorMessage));
        }

        public void SetErrorStatus(string message)
        {
            ServiceStatus = ServiceStatus.Error;
            ErrorMessage = message;

            StatusChanged?.Invoke(this, EventArgs.Empty);

            OnPropertyChanged(nameof(ServiceStatus));
            OnPropertyChanged(nameof(ErrorMessage));
        }
    }
}
