using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Models.AI;
using System;
using System.Text.Json.Serialization;

namespace PowerPad.WinUI.ViewModels.AI
{
    public partial class AIModelViewModel(AIModel aiModel, bool enabled = false, bool available = true, bool downloading = false) : ObservableObject
    {
        private readonly AIModel _aiModel = aiModel;

        [JsonConstructor]
        public AIModelViewModel(string name, ModelProvider modelProvider, string infoUrl, long? size = null, string? displayName = null, bool enabled = false)
            : this(new(name, modelProvider, infoUrl, size, displayName), enabled)
        {
        }

        public string Name => _aiModel.Name;

        public ModelProvider ModelProvider => _aiModel.ModelProvider;

        public string? InfoUrl => _aiModel.InfoUrl;

        [ObservableProperty]
        private bool _enabled = enabled;

#pragma warning disable CS0657
        [ObservableProperty]
        [property: JsonIgnore]
        private bool _available = available;

        [ObservableProperty]
        [property: JsonIgnore]
        private bool _downloading = downloading;

        [ObservableProperty]
        [property: JsonIgnore]
        private double _progress;

        [ObservableProperty]
        [property: JsonIgnore]
        private bool _downloadError;
#pragma warning restore CS0657

        public long? Size => _aiModel.Size;

        public string? DisplayName => _aiModel.DisplayName;

        [JsonIgnore]
        public bool CanAdd => !Available && !Downloading;

        public AIModel GetRecord() => _aiModel;

        [JsonIgnore]
        public string CardName => DisplayName ?? Name;

        public override bool Equals(object? other)
        {
            if (other is null) return false;

            if (other is AIModelViewModel otherAIViewModel)
            {
                if (ReferenceEquals(this, other)) return true;
                return GetRecord() == otherAIViewModel.GetRecord();
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return GetRecord().GetHashCode();
        }

        public static bool operator ==(AIModelViewModel? left, AIModelViewModel? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(AIModelViewModel? left, AIModelViewModel? right)
        {
            return !(left == right);
        }

        partial void OnAvailableChanged(bool value) => OnPropertyChanged(nameof(CanAdd));
        partial void OnDownloadingChanged(bool value) => OnPropertyChanged(nameof(CanAdd));

        public void UpdateDownloadProgess(double progress)
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

        public void UpdateDownloadError(Exception _)
        {
            //TODO: Handle error or show message
            DownloadError = true;
        }
    }
}