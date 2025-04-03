using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;
using PowerPad.Core.Models.FileSystem;
using PowerPad.WinUI.ViewModels.AI;
using PowerPad.WinUI.ViewModels.FileSystem;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.WinUI.ViewModels.Chat
{
    public partial class ChatViewModel : ObservableObject
    {
        [ObservableProperty]
        private AIModelViewModel? _model;

        [ObservableProperty]
        private AIParameters? _parameters;

        public required ObservableCollection<MessageViewModel> Messages { get; init; }
    }
}
