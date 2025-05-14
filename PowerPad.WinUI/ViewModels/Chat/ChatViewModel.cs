using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PowerPad.WinUI.ViewModels.AI;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace PowerPad.WinUI.ViewModels.Chat
{
    /// <summary>
    /// ViewModel for managing chat functionality, including messages, AI parameters, and commands.
    /// </summary>
    public partial class ChatViewModel : ObservableObject
    {
        /// <summary>
        /// Gets or sets the AI model associated with the chat.
        /// </summary>
        [ObservableProperty]
        public partial AIModelViewModel? Model { get; set; }

        /// <summary>
        /// Gets or sets the AI parameters used for configuring the chat behavior.
        /// </summary>
        [ObservableProperty]
        public partial AIParametersViewModel? Parameters { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the agent associated with the chat.
        /// </summary>
        [ObservableProperty]
        public partial Guid? AgentId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether there is an error in the chat.
        /// </summary>
        [ObservableProperty]
        public partial bool ChatError { get; set; }

        /// <summary>
        /// Gets the collection of messages in the chat.
        /// </summary>
        /// <remarks>
        /// The collection is initialized with a handler for monitoring changes.
        /// </remarks>
        public required ObservableCollection<MessageViewModel> Messages
        {
            get;
            init
            {
                field = value;
                field.CollectionChanged += MessageCollectionChangedHandler;
            }
        }

        /// <summary>
        /// Command to remove the last message from the chat.
        /// </summary>
        public IRelayCommand RemoveLastMessageCommand { get; }

        /// <summary>
        /// Command to clear all messages from the chat.
        /// </summary>
        public IRelayCommand ClearMessagesCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatViewModel"/> class.
        /// </summary>
        public ChatViewModel()
        {
            RemoveLastMessageCommand = new RelayCommand(RemoveLastMessage);
            ClearMessagesCommand = new RelayCommand(ClearMessages);
        }

        /// <summary>
        /// Removes the last message from the chat if there are more than one message.
        /// </summary>
        public void RemoveLastMessage()
        {
            if (Messages.Count > 1)
            {
                Messages.RemoveAt(Messages.Count - 1);
                Messages.RemoveAt(Messages.Count - 1);
            }
        }

        /// <summary>
        /// Clears all messages from the chat.
        /// </summary>
        public void ClearMessages() => Messages.Clear();

        /// <summary>
        /// Handles changes to the <see cref="Messages"/> collection and updates the <see cref="ChatError"/> property.
        /// </summary>
        /// <param name="_">The source of the event (not used).</param>
        /// <param name="eventArgs">The event arguments containing details about the change.</param>
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
    }
}