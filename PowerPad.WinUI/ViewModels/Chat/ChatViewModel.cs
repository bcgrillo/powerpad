using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic;
using PowerPad.WinUI.ViewModels.AI;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace PowerPad.WinUI.ViewModels.Chat
{
    public partial class ChatViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial AIModelViewModel? Model { get; set; }

        [ObservableProperty]
        public partial AIParametersViewModel? Parameters { get; set; }

        [ObservableProperty]
        public partial bool ChatError { get; set; }

        public required ObservableCollection<MessageViewModel> Messages
        {
            get;
            init
            {
                field = value;
                field.CollectionChanged += MessageCollectionChangedHandler;
            }
        }

        public IRelayCommand RemoveLastMessageCommand;
        public IRelayCommand ClearMessagesCommand;

        public ChatViewModel()
        {
            RemoveLastMessageCommand = new RelayCommand(RemoveLastMessage);
            ClearMessagesCommand = new RelayCommand(ClearMessages);
        }

        private void MessageCollectionChangedHandler(object? _, NotifyCollectionChangedEventArgs eventArgs)
        {
            ChatError = eventArgs.Action switch
            {
                NotifyCollectionChangedAction.Add => ChatError || eventArgs.NewItems!.Cast<MessageViewModel>().Any(m => m.ErrorMessage != null),
                NotifyCollectionChangedAction.Remove => ChatError && Messages.Any(m => m.ErrorMessage != null),
                NotifyCollectionChangedAction.Reset => false,
                _ => throw new NotImplementedException("Only Add and Remove actions are supported."),
            };
        }

        public void RemoveLastMessage()
        {
            if (Messages.Count > 1)
            {
                Messages.RemoveAt(Messages.Count - 1);
                Messages.RemoveAt(Messages.Count - 1);
            }
        }

        public void ClearMessages() => Messages.Clear();
    }
}