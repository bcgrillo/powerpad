using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PowerPad.WinUI.ViewModels.Settings
{
    /// <summary>
    /// ViewModel for managing the configuration of an AI service.
    /// </summary>
    public partial class AIServiceConfigViewModel(AIServiceConfig aiServiceConfig) : ObservableObject
    {
        private readonly AIServiceConfig _aiServiceConfig = aiServiceConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="AIServiceConfigViewModel"/> class with the specified base URL and key.
        /// </summary>
        /// <param name="baseUrl">The base URL of the AI service.</param>
        /// <param name="key">The API key for accessing the AI service.</param>
        [JsonConstructor]
        public AIServiceConfigViewModel(string? baseUrl, string? key)
            : this(new()
            {
                BaseUrl = baseUrl,
                Key = key
            })
        {
        }

        /// <summary>
        /// Gets or sets the base URL of the AI service.
        /// </summary>
        public string? BaseUrl
        {
            get => _aiServiceConfig.BaseUrl;
            set
            {
                SetProperty(_aiServiceConfig.BaseUrl, value, _aiServiceConfig, (x, y) => x.BaseUrl = y);
                ConfigChanged?.Invoke(this, new(nameof(BaseUrl)));
            }
        }

        /// <summary>
        /// Gets or sets the API key for accessing the AI service.
        /// </summary>
        public string? Key
        {
            get => _aiServiceConfig.Key;
            set
            {
                SetProperty(_aiServiceConfig.Key, value, _aiServiceConfig, (x, y) => x.Key = y);
                ConfigChanged?.Invoke(this, new(nameof(Key)));
            }
        }

        /// <summary>
        /// Gets the current status of the AI service.
        /// </summary>
        [JsonIgnore]
        public ServiceStatus ServiceStatus { get; private set; } = ServiceStatus.Unknown;

        /// <summary>
        /// Gets the error message, if any, related to the AI service.
        /// </summary>
        [JsonIgnore]
        public string? ErrorMessage { get; private set; }

        /// <summary>
        /// Event triggered when the configuration changes.
        /// </summary>
        public event EventHandler<PropertyChangedEventArgs>? ConfigChanged;

        /// <summary>
        /// Event triggered when the status of the AI service changes.
        /// </summary>
        public event EventHandler? StatusChanged;

        /// <summary>
        /// Retrieves the current configuration record of the AI service.
        /// </summary>
        /// <returns>The current <see cref="AIServiceConfig"/> instance.</returns>
        public AIServiceConfig GetRecord() => _aiServiceConfig;

        /// <summary>
        /// Tests the connection to the AI service.
        /// </summary>
        /// <param name="aiService">The AI service to test the connection with.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Resets the status of the AI service to unknown.
        /// </summary>
        public void ResetStatus()
        {
            ServiceStatus = ServiceStatus.Unknown;
            ErrorMessage = null;

            StatusChanged?.Invoke(this, EventArgs.Empty);

            OnPropertyChanged(nameof(ServiceStatus));
            OnPropertyChanged(nameof(ErrorMessage));
        }

        /// <summary>
        /// Sets the status of the AI service to error with a specified message.
        /// </summary>
        /// <param name="message">The error message to set.</param>
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
