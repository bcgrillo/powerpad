using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.WinUI.ViewModels.AI;
using System.Collections.ObjectModel;

namespace PowerPad.WinUI.ViewModels.Chat
{
    public partial class ChatViewModel : ObservableObject
    {
        [ObservableProperty]
        private AIModelViewModel? _model;

        [ObservableProperty]
        private AIParametersViewModel? _parameters;

        public required ObservableCollection<MessageViewModel> Messages { get; init; }
    }
}
