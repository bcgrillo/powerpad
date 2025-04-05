using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Models.AI;
using System.Text.Json.Serialization;

namespace PowerPad.WinUI.ViewModels.AI
{
    public partial class AIModelViewModel(AIModel aiModel, bool enabled = false, bool available = true, bool downloading = false) : ObservableObject
    {
        private readonly AIModel _aiModel = aiModel;

        [JsonConstructor]
        public AIModelViewModel(string Name, ModelProvider ModelProvider, long? Size = null, string? DisplayName = null, bool enabled = false)
            : this(new(Name, ModelProvider, Size, DisplayName), enabled)
        {
        }

        public string Name => _aiModel.Name;

        public ModelProvider ModelProvider => _aiModel.ModelProvider;

        [ObservableProperty]
        private bool _enabled = enabled;

        [ObservableProperty]
        [property: JsonIgnore]
        private bool _available = available;

        [ObservableProperty]
        [property: JsonIgnore]
        private bool _downloading = downloading;

        public long? Size => _aiModel.Size;

        public string? DisplayName => _aiModel.DisplayName;

        [JsonIgnore]
        public bool CanAdd => !Available && !Downloading;

        public AIModel GetRecord() => _aiModel;

        [JsonIgnore]
        public string? CardName => DisplayName ?? Name;

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
    }
}