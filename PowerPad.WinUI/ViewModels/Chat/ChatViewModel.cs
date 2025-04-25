using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.WinUI.ViewModels.AI;
using System.Collections.ObjectModel;

namespace PowerPad.WinUI.ViewModels.Chat
{
    public partial class ChatViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial AIModelViewModel? Model { get; set; }

        [ObservableProperty]
        public partial AIParametersViewModel? Parameters { get; set; }

        public required ObservableCollection<MessageViewModel> Messages { get; init; }
    }
}
