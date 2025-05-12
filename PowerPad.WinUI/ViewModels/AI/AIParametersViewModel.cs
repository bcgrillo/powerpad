using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Models.AI;
using System.Text.Json.Serialization;

namespace PowerPad.WinUI.ViewModels.AI
{
    /// <summary>
    /// ViewModel for managing AI parameters, providing data binding and change notification.
    /// </summary>
    public class AIParametersViewModel(AIParameters aiParameters) : ObservableObject
    {
        private readonly AIParameters _aiParameters = aiParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="AIParametersViewModel"/> class with optional parameters.
        /// </summary>
        /// <param name="systemPrompt">The system prompt for the AI.</param>
        /// <param name="temperature">The temperature value controlling randomness.</param>
        /// <param name="topP">The Top-P value controlling cumulative probability for token sampling.</param>
        /// <param name="maxOutputTokens">The maximum number of tokens allowed in the output response.</param>
        /// <param name="maxConversationLength">The maximum length of a conversation in terms of messages.</param>
        [JsonConstructor]
        public AIParametersViewModel(string? systemPrompt = null, float? temperature = null, float? topP = null, int? maxOutputTokens = null, int? maxConversationLength = null)
                   : this(new()
                   {
                       SystemPrompt = systemPrompt,
                       Temperature = temperature,
                       TopP = topP,
                       MaxOutputTokens = maxOutputTokens,
                       MaxConversationLength = maxConversationLength
                   })
        {
        }

        /// <summary>
        /// Gets or sets the system prompt, which serves as the initial instruction or context for the AI.
        /// </summary>
        public string? SystemPrompt
        {
            get => _aiParameters.SystemPrompt;
            set => SetProperty(_aiParameters.SystemPrompt, value, _aiParameters, (x, y) => x.SystemPrompt = y);
        }

        /// <summary>
        /// Gets or sets the temperature value, which controls the randomness of the chat responses.
        /// </summary>
        public float? Temperature
        {
            get => _aiParameters.Temperature;
            set => SetProperty(_aiParameters.Temperature, value, _aiParameters, (x, y) => x.Temperature = y);
        }

        /// <summary>
        /// Gets or sets the Top-P value, which controls the cumulative probability for token sampling.
        /// </summary>
        public float? TopP
        {
            get => _aiParameters.TopP;
            set => SetProperty(_aiParameters.TopP, value, _aiParameters, (x, y) => x.TopP = y);
        }

        /// <summary>
        /// Gets or sets the maximum number of tokens allowed in the output response.
        /// </summary>
        public int? MaxOutputTokens
        {
            get => _aiParameters.MaxOutputTokens;
            set => SetProperty(_aiParameters.MaxOutputTokens, value, _aiParameters, (x, y) => x.MaxOutputTokens = y);
        }

        /// <summary>
        /// Gets or sets the maximum length of a conversation in terms of the number of messages.
        /// </summary>
        public int? MaxConversationLength
        {
            get => _aiParameters.MaxConversationLength;
            set => SetProperty(_aiParameters.MaxConversationLength, value, _aiParameters, (x, y) => x.MaxConversationLength = y);
        }

        /// <summary>
        /// Retrieves the underlying <see cref="AIParameters"/> record.
        /// </summary>
        /// <returns>The current <see cref="AIParameters"/> instance.</returns>
        public AIParameters GetRecord() => _aiParameters;

        /// <summary>
        /// Updates the ViewModel with a new <see cref="AIParameters"/> record.
        /// </summary>
        /// <param name="parameters">The new AI parameters to set.</param>
        public void SetRecord(AIParameters parameters)
        {
            SystemPrompt = parameters.SystemPrompt;
            Temperature = parameters.Temperature;
            TopP = parameters.TopP;
            MaxOutputTokens = parameters.MaxOutputTokens;
            MaxConversationLength = parameters.MaxConversationLength;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if the objects are equal; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is not AIParametersViewModel other)
                return false;

            if (ReferenceEquals(this, other)) return true;

            return GetRecord() == other.GetRecord();
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return GetRecord().GetHashCode();
        }

        /// <summary>
        /// Equality operator for comparing two <see cref="AIParametersViewModel"/> instances.
        /// </summary>
        /// <param name="left">The left-hand instance.</param>
        /// <param name="right">The right-hand instance.</param>
        /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(AIParametersViewModel? left, AIParametersViewModel? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator for comparing two <see cref="AIParametersViewModel"/> instances.
        /// </summary>
        /// <param name="left">The left-hand instance.</param>
        /// <param name="right">The right-hand instance.</param>
        /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(AIParametersViewModel? left, AIParametersViewModel? right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Creates a shallow copy of the current <see cref="AIParametersViewModel"/> instance.
        /// </summary>
        /// <returns>A new <see cref="AIParametersViewModel"/> instance with the same values.</returns>
        public AIParametersViewModel Copy()
        {
            // Shallow copy
            return new(GetRecord() with { });
        }
    }
}
