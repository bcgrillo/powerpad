using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.AI;
using System;
using System.Text.Json.Serialization;

namespace PowerPad.WinUI.ViewModels.Chat
{
    public partial class MessageViewModel(string? content, DateTime dateTime, ChatRole role, string? reasoning = null, string? errorMessage = null) : ObservableObject
    {
        [ObservableProperty]
        public partial string? Content { get; set; } = content;

        public DateTime DateTime { get; private init; } = dateTime;

        public ChatRole Role { get; set; } = role;

        [ObservableProperty]
        public partial string? Reasoning { get; set; } = reasoning;

        [ObservableProperty]
        [JsonIgnore]
        public partial bool Loading { get; set; }

        [ObservableProperty]
        [JsonIgnore]
        public partial string LoadingMessage { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string? ErrorMessage { get; set; } = errorMessage;

        public override string ToString()
        {
            return $"{DateTime} {Content}";
        }
    }
}
