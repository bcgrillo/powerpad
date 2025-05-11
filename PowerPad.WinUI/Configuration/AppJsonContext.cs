using PowerPad.WinUI.ViewModels.Agents;
using PowerPad.WinUI.ViewModels.AI;
using PowerPad.WinUI.ViewModels.Chat;
using PowerPad.WinUI.ViewModels.Settings;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace PowerPad.WinUI.Configuration
{
    /// <summary>
    /// Provides a custom JSON serialization context for the application.
    /// This class is used to predefine the types that can be serialized/deserialized
    /// using System.Text.Json for improved performance and reduced runtime reflection.
    /// </summary>
    [JsonSerializable(typeof(GeneralSettingsViewModel))]
    [JsonSerializable(typeof(AIServiceConfigViewModel))]
    [JsonSerializable(typeof(ModelsSettingsViewModel))]
    [JsonSerializable(typeof(AIParametersViewModel))]
    [JsonSerializable(typeof(ObservableCollection<AIModelViewModel>))]
    [JsonSerializable(typeof(AIModelViewModel))]
    [JsonSerializable(typeof(List<string>))]
    [JsonSerializable(typeof(ObservableCollection<AgentViewModel>))]
    [JsonSerializable(typeof(AgentViewModel))]
    [JsonSerializable(typeof(ObservableCollection<string>))]
    [JsonSerializable(typeof(ChatViewModel))]
    [JsonSerializable(typeof(ObservableCollection<MessageViewModel>))]
    [JsonSerializable(typeof(MessageViewModel))]
    public partial class AppJsonContext : JsonSerializerContext
    {
        /// <summary>
        /// Gets a custom instance of <see cref="AppJsonContext"/> configured with
        /// predefined JSON serializer options.
        /// </summary>
        public static AppJsonContext Custom { get; } = new(ConfigConstants.JSON_SERIALIZER_OPTIONS);
    }
}