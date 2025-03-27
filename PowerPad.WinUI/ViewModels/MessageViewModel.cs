using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.AI;
using System;

namespace PowerPad.WinUI.ViewModels
{
    public partial class MessageViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _content;

        public DateTime DateTime { get; private set; }

        public ChatRole Role { get; set; }

        public MessageViewModel(string content, DateTime dateTime, ChatRole role)
        {
            Content = content;
            DateTime = dateTime;
            Role = role;
        }

        public override string ToString()
        {
            return $"{DateTime} {Content}";
        }
    }
}
