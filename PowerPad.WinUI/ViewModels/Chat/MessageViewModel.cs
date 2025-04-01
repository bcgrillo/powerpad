using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.AI;
using System;

namespace PowerPad.WinUI.ViewModels.Chat
{
    public partial class MessageViewModel(string content, DateTime dateTime, ChatRole role) : ObservableObject
    {
        [ObservableProperty]
        private string _content = content;

        public DateTime DateTime { get; private init; } = dateTime;

        public ChatRole Role { get; set; } = role;

        public override string ToString()
        {
            return $"{DateTime} {Content}";
        }
    }
}
