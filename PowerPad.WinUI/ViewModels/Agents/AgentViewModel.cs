using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using PowerPad.Core.Models.AI;
using PowerPad.WinUI.Helpers;
using System;
using System.Text.Json.Serialization;

namespace PowerPad.WinUI.ViewModels.Agents
{
    /// <summary>
    /// ViewModel representing an AI Agent with its associated properties and behaviors.
    /// </summary>
    public partial class AgentViewModel : ObservableObject
    {
        private readonly Agent _agent;
        private ImageSource? _iconElementSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentViewModel"/> class.
        /// </summary>
        /// <param name="agent">The agent model containing core data.</param>
        /// <param name="icon">The icon associated with the agent.</param>
        public AgentViewModel(Agent agent, AgentIcon icon)
        {
            _agent = agent;
            Icon = icon;

            Id = Guid.NewGuid();
            ShowInNotes = true;
            ShowInChats = true;

            if (icon.Type == AgentIconType.Base64Image)
                _iconElementSource = Base64ImageHelper.LoadImageFromBase64(Icon.Source, 16);
            else
                _iconElementSource = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentViewModel"/> class with JSON deserialization.
        /// </summary>
        /// <param name="id">The unique identifier of the agent.</param>
        /// <param name="name">The name of the agent.</param>
        /// <param name="prompt">The prompt used by the agent.</param>
        /// <param name="promptParameterName">The name of the parameter used in the prompt, if any.</param>
        /// <param name="promptParameterDescription">The description of the parameter used in the prompt, if any.</param>
        /// <param name="aiModel">The AI model associated with the agent.</param>
        /// <param name="temperature">The temperature value for AI randomness.</param>
        /// <param name="topP">The Top-P value for token sampling.</param>
        /// <param name="maxOutputTokens">The maximum number of tokens in the output.</param>
        /// <param name="icon">The icon associated with the agent.</param>
        /// <param name="showInNotes">Indicates whether the agent is shown in notes.</param>
        /// <param name="showInChats">Indicates whether the agent is shown in chats.</param>
        [JsonConstructor]
        public AgentViewModel(Guid id,
                              string name,
                              string prompt,
                              string? promptParameterName,
                              string? promptParameterDescription,
                              AIModel? aiModel,
                              float? temperature,
                              float? topP,
                              int? maxOutputTokens,
                              AgentIcon icon,
                              bool showInNotes,
                              bool showInChats)
            : this(new()
            {
                Name = name,
                Prompt = prompt,
                PromptParameterName = promptParameterName,
                PromptParameterDescription = promptParameterDescription,
                AIModel = aiModel,
                Temperature = temperature,
                TopP = topP,
                MaxOutputTokens = maxOutputTokens
            }, icon)
        {
            Id = id;
            ShowInNotes = showInNotes;
            ShowInChats = showInChats;
        }

        /// <summary>
        /// Gets or sets the unique identifier of the agent.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the agent.
        /// </summary>
        public string Name
        {
            get => _agent.Name;
            set => SetProperty(_agent.Name, value, _agent, (x, y) => x.Name = y);
        }

        /// <summary>
        /// Gets or sets the prompt used by the agent.
        /// </summary>
        public string Prompt
        {
            get => _agent.Prompt;
            set => SetProperty(_agent.Prompt, value, _agent, (x, y) => x.Prompt = y);
        }

        /// <summary>
        /// Gets or sets the name of the parameter used in the prompt, if any.
        /// </summary>
        public string? PromptParameterName
        {
            get => _agent.PromptParameterName;
            set => SetProperty(_agent.PromptParameterName, value, _agent, (x, y) => x.PromptParameterName = y);
        }

        /// <summary>
        /// Gets or sets the description of the parameter used in the prompt, if any.
        /// </summary>
        public string? PromptParameterDescription
        {
            get => _agent.PromptParameterDescription;
            set => SetProperty(_agent.PromptParameterDescription, value, _agent, (x, y) => x.PromptParameterDescription = y);
        }

        /// <summary>
        /// Gets or sets the AI model associated with the agent.
        /// </summary>
        public AIModel? AIModel
        {
            get => _agent.AIModel;
            set => SetProperty(_agent.AIModel, value, _agent, (x, y) => x.AIModel = y);
        }

        /// <summary>
        /// Gets or sets the temperature value for AI randomness.
        /// </summary>
        public float? Temperature
        {
            get => _agent.Temperature;
            set => SetProperty(_agent.Temperature, value, _agent, (x, y) => x.Temperature = y);
        }

        /// <summary>
        /// Gets or sets the Top-P value for token sampling.
        /// </summary>
        public float? TopP
        {
            get => _agent.TopP;
            set => SetProperty(_agent.TopP, value, _agent, (x, y) => x.TopP = y);
        }

        /// <summary>
        /// Gets or sets the maximum number of tokens in the output.
        /// </summary>
        public int? MaxOutputTokens
        {
            get => _agent.MaxOutputTokens;
            set => SetProperty(_agent.MaxOutputTokens, value, _agent, (x, y) => x.MaxOutputTokens = y);
        }

        /// <summary>
        /// Gets or sets the icon associated with the agent.
        /// </summary>
        [ObservableProperty]
        public partial AgentIcon Icon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the agent is shown in notes.
        /// </summary>
        [ObservableProperty]
        public partial bool ShowInNotes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the agent is shown in chats.
        /// </summary>
        [ObservableProperty]
        public partial bool ShowInChats { get; set; }

        /// <summary>
        /// Gets a value indicating whether the agent allows drag-and-drop operations.
        /// </summary>
        [JsonIgnore]
        public bool AllowDrop { get; private init; } = false; // AllowDrop = false only works with binding to a property, not with a constant

        /// <summary>
        /// Gets or sets a value indicating whether the agent is selected.
        /// </summary>
        [ObservableProperty]
        [JsonIgnore]
        public partial bool IsSelected { get; set; }

        /// <summary>
        /// Gets the icon element representation of the agent's icon.
        /// </summary>
        [JsonIgnore]
        public IconElement IconElement => Icon.Type switch
        {
            AgentIconType.Base64Image => new ImageIcon { Source = _iconElementSource },
            AgentIconType.FontIconGlyph => Icon.Color.HasValue
                ? new FontIcon { Glyph = Icon.Source, Foreground = new SolidColorBrush(Icon.Color.Value) }
                : new FontIcon { Glyph = Icon.Source },
            _ => throw new NotImplementedException()
        };

        /// <summary>
        /// Gets a value indicating whether the agent has a prompt parameter.
        /// </summary>
        [JsonIgnore]
        public bool HasPromptParameter => !string.IsNullOrEmpty(PromptParameterName);

        /// <summary>
        /// Gets a value indicating whether the agent has AI parameters configured.
        /// </summary>
        [JsonIgnore]
        public bool HasAIParameters => Temperature.HasValue;

        /// <summary>
        /// Retrieves the underlying agent record.
        /// </summary>
        /// <returns>The <see cref="Agent"/> record.</returns>
        public Agent GetRecord() => _agent;

        /// <summary>
        /// Updates the agent's properties with the values from the provided record.
        /// </summary>
        /// <param name="agent">The agent record containing updated values.</param>
        public void SetRecord(Agent agent)
        {
            Name = agent.Name;
            Prompt = agent.Prompt;
            PromptParameterName = agent.PromptParameterName;
            PromptParameterDescription = agent.PromptParameterDescription;
            AIModel = agent.AIModel;
            Temperature = agent.Temperature;
            TopP = agent.TopP;
            MaxOutputTokens = agent.MaxOutputTokens;
        }

        partial void OnIconChanged(AgentIcon oldValue, AgentIcon newValue)
        {
            if (Icon.Type == AgentIconType.Base64Image)
                _iconElementSource = Base64ImageHelper.LoadImageFromBase64(Icon.Source, 16);
            else
                _iconElementSource = null;

            OnPropertyChanged(nameof(IconElement));
        }

        /// <summary>
        /// Creates a shallow copy of the current <see cref="AgentViewModel"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="AgentViewModel"/> with copied values.</returns>
        public AgentViewModel Copy()
        {
            // Shallow copy
            var copy = new AgentViewModel(GetRecord() with { }, Icon)
            {
                ShowInNotes = this.ShowInNotes,
                ShowInChats = this.ShowInChats
            };

            return copy;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if the objects are equal; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is not AgentViewModel other)
                return false;

            if (ReferenceEquals(this, other)) return true;

            return GetRecord() == other.GetRecord() &&
                   Icon == other.Icon &&
                   ShowInNotes == other.ShowInNotes &&
                   ShowInChats == other.ShowInChats;
        }

        /// <summary>
        /// Gets the hash code for the current instance.
        /// </summary>
        /// <returns>The hash code for the current instance.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(GetRecord(), Icon, ShowInNotes, ShowInChats);
        }

        /// <summary>
        /// Determines whether two <see cref="AgentViewModel"/> instances are equal.
        /// </summary>
        /// <param name="left">The first instance to compare.</param>
        /// <param name="right">The second instance to compare.</param>
        /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(AgentViewModel? left, AgentViewModel? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="AgentViewModel"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first instance to compare.</param>
        /// <param name="right">The second instance to compare.</param>
        /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(AgentViewModel? left, AgentViewModel? right)
        {
            return !(left == right);
        }
    }
}
