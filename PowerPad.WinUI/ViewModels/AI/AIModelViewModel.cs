using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Models.AI;
using System;
using System.Text.Json.Serialization;
using System.Threading;

namespace PowerPad.WinUI.ViewModels.AI
{
    /// <summary>
    /// ViewModel for representing and managing the state of an AI model.
    /// </summary>
    /// <param name="aiModel">The AI model associated with this ViewModel.</param>
    /// <param name="enabled">Indicates whether the AI model is enabled.</param>
    /// <param name="available">Indicates whether the AI model is available for use.</param>
    /// <param name="downloading">Indicates whether the AI model is currently being downloaded.</param>
    public partial class AIModelViewModel(AIModel aiModel, bool enabled = false, bool available = true, bool downloading = false) : ObservableObject
    {
        private readonly AIModel _aiModel = aiModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="AIModelViewModel"/> class using JSON deserialization.
        /// </summary>
        /// <param name="name">The unique name of the AI model.</param>
        /// <param name="modelProvider">The provider of the AI model (e.g., OpenAI, HuggingFace).</param>
        /// <param name="infoUrl">An optional URL with more information about the model.</param>
        /// <param name="size">The optional size of the model in bytes.</param>
        /// <param name="displayName">An optional display name for the model.</param>
        /// <param name="enabled">Indicates whether the AI model is enabled.</param>
        [JsonConstructor]
        public AIModelViewModel(string name, ModelProvider modelProvider, string infoUrl, long? size = null, string? displayName = null, bool enabled = false)
            : this(new(name, modelProvider, infoUrl, size, displayName), enabled)
        {
        }

        /// <summary>
        /// Gets the unique name of the AI model.
        /// </summary>
        public string Name => _aiModel.Name;

        /// <summary>
        /// Gets the provider of the AI model.
        /// </summary>
        public ModelProvider ModelProvider => _aiModel.ModelProvider;

        /// <summary>
        /// Gets an optional URL with more information about the model.
        /// </summary>
        public string? InfoUrl => _aiModel.InfoUrl;

        /// <summary>
        /// Gets or sets a value indicating whether the AI model is enabled.
        /// </summary>
        [ObservableProperty]
        public partial bool Enabled { get; set; } = enabled;

        /// <summary>
        /// Gets or sets a value indicating whether the AI model is available for use.
        /// </summary>
        [ObservableProperty]
        [JsonIgnore]
        public partial bool Available { get; set; } = available;

        /// <summary>
        /// Gets or sets a value indicating whether the AI model is currently being downloaded.
        /// </summary>
        [ObservableProperty]
        [JsonIgnore]
        public partial bool Downloading { get; set; } = downloading;

        /// <summary>
        /// Gets or sets the download progress of the AI model.
        /// </summary>
        [ObservableProperty]
        [JsonIgnore]
        public partial double Progress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether there was an error during the download process.
        /// </summary>
        [ObservableProperty]
        [JsonIgnore]
        public partial bool DownloadError { get; set; }

        /// <summary>
        /// Gets or sets the cancellation token source for the download operation.
        /// </summary>
        [JsonIgnore]
        public CancellationTokenSource? DownloadCancelationToken { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the size of the model is too large for execution.
        /// </summary>
        [JsonIgnore]
        public bool IsSizeTooLargeForExecution { get; set; }

        /// <summary>
        /// Gets the optional size of the AI model in bytes.
        /// </summary>
        public long? Size => _aiModel.Size;

        /// <summary>
        /// Gets the optional display name of the AI model.
        /// </summary>
        public string? DisplayName => _aiModel.DisplayName;

        /// <summary>
        /// Gets a value indicating whether the AI model can be added.
        /// </summary>
        [JsonIgnore]
        public bool CanAdd => !Available && !Downloading && !IsSizeTooLargeForExecution;

        /// <summary>
        /// Retrieves the underlying AI model record.
        /// </summary>
        /// <returns>The associated <see cref="AIModel"/> record.</returns>
        public AIModel GetRecord() => _aiModel;

        /// <summary>
        /// Gets the display name or the unique name of the AI model.
        /// </summary>
        [JsonIgnore]
        public string CardName => DisplayName ?? Name;

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the objects are equal; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is not AIModelViewModel other)
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
        /// Determines whether two <see cref="AIModelViewModel"/> instances are equal.
        /// </summary>
        /// <param name="left">The first instance to compare.</param>
        /// <param name="right">The second instance to compare.</param>
        /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(AIModelViewModel? left, AIModelViewModel? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="AIModelViewModel"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first instance to compare.</param>
        /// <param name="right">The second instance to compare.</param>
        /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(AIModelViewModel? left, AIModelViewModel? right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Called when the <see cref="Available"/> property changes.
        /// </summary>
        /// <param name="value">The new value of the <see cref="Available"/> property.</param>
        partial void OnAvailableChanged(bool value) => OnPropertyChanged(nameof(CanAdd));

        /// <summary>
        /// Called when the <see cref="Downloading"/> property changes.
        /// </summary>
        /// <param name="value">The new value of the <see cref="Downloading"/> property.</param>
        partial void OnDownloadingChanged(bool value) => OnPropertyChanged(nameof(CanAdd));

        /// <summary>
        /// Updates the download progress of the AI model.
        /// </summary>
        /// <param name="progress">The current download progress as a percentage.</param>
        public void UpdateDownloadProgress(double progress)
        {
            if (progress < 100)
            {
                Progress = progress;
            }
            else
            {
                Progress = 100;
                Downloading = false;
                Available = true;
                Enabled = true;
            }
        }

        /// <summary>
        /// Sets the download error state to <c>true</c>.
        /// </summary>
        public void SetDownloadError() => DownloadError = true;
    }
}