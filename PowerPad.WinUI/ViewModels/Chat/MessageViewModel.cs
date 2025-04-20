using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.AI;
using System;
using System.Text.Json.Serialization;

namespace PowerPad.WinUI.ViewModels.Chat
{
    public partial class MessageViewModel(string? content, DateTime dateTime, ChatRole role, string? reasoning = null) : ObservableObject
    {
        [ObservableProperty]
        private string? _content = content;

        public DateTime DateTime { get; private init; } = dateTime;

        public ChatRole Role { get; set; } = role;

        [ObservableProperty]
        private string? _reasoning = reasoning;

#pragma warning disable CS0657
        [ObservableProperty]
        [property: JsonIgnore]
        private bool _loading;

        [ObservableProperty]
        [property: JsonIgnore]
        private string _loadingMessage = string.Empty;
#pragma warning restore CS0657

        public override string ToString()
        {
            return $"{DateTime} {Content}";
        }
    }
}
