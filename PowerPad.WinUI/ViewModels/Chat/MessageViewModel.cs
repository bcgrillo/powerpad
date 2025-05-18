using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.AI;
using System;
using System.Text.Json.Serialization;

namespace PowerPad.WinUI.ViewModels.Chat
{
    /// <summary>
    /// Represents a message in the chat, including its content, timestamp, role, and additional metadata.
    /// </summary>
    /// <param name="content">The content of the message.</param>
    /// <param name="dateTime">The timestamp of when the message was created.</param>
    /// <param name="role">The role of the message sender (e.g., user, assistant).</param>
    /// <param name="reasoning">Optional reasoning or explanation associated with the message.</param>
    /// <param name="errorMessage">Optional error message if the message encountered an issue.</param>
    public partial class MessageViewModel(string? content, DateTime dateTime, ChatRole role, string? reasoning = null, string? errorMessage = null) : ObservableObject
    {
        /// <summary>
        /// Gets or sets the content of the message.
        /// </summary>
        [ObservableProperty]
        public partial string? Content { get; set; } = content;

        /// <summary>
        /// Gets the timestamp of when the message was created.
        /// </summary>
        public DateTime DateTime { get; private init; } = dateTime;

        /// <summary>
        /// Gets or sets the role of the message sender (e.g., user, assistant).
        /// </summary>
        public ChatRole Role { get; private init; } = role;

        /// <summary>
        /// Gets or sets the reasoning or explanation associated with the message.
        /// </summary>
        [ObservableProperty]
        public partial string? Reasoning { get; set; } = reasoning;

        /// <summary>
        /// Gets or sets a value indicating whether the message is in a loading state.
        /// </summary>
        [ObservableProperty]
        [JsonIgnore]
        public partial bool Loading { get; set; }

        /// <summary>
        /// Gets or sets the loading message displayed during the loading state.
        /// </summary>
        [ObservableProperty]
        [JsonIgnore]
        public partial string LoadingMessage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets an error message if the message encountered an issue.
        /// </summary>
        [ObservableProperty]
        public partial string? ErrorMessage { get; set; } = errorMessage;
    }
}
