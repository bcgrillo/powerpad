using PowerPad.WinUI.ViewModels.Agents;
using PowerPad.WinUI.ViewModels.Chat;
using PowerPad.WinUI.ViewModels.Settings;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PowerPad.WinUI.Configuration
{
    [JsonSerializable(typeof(GeneralSettingsViewModel))]
    [JsonSerializable(typeof(ModelsSettingsViewModel))]
    [JsonSerializable(typeof(List<string>))]
    [JsonSerializable(typeof(ObservableCollection<AgentViewModel>))]
    [JsonSerializable(typeof(ObservableCollection<string>))]
    [JsonSerializable(typeof(ChatViewModel))]
    public partial class AppJsonContext : JsonSerializerContext
    {
        public static AppJsonContext Custom { get; } = new(ConfigConstants.JSON_SERIALIZER_OPTIONS);
    }
}