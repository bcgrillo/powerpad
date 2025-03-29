using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using PowerPad.Core.Models;
using PowerPad.WinUI.Messages;
using System.Text.Json.Serialization;

namespace PowerPad.WinUI.ViewModels.AI
{
    public partial class AIModelViewModel(AIModel aiModel, bool enabled = false) : ObservableObject
    {
        private readonly AIModel _aiModel = aiModel;

        [JsonConstructor]
        public AIModelViewModel(string Name, ModelProvider ModelProvider, long? Size = null, string? DisplayName = null, bool enabled = false)
            : this(new AIModel(Name, ModelProvider, Size, DisplayName), enabled)
        {
        }

        public string Name => _aiModel.Name;

        public ModelProvider ModelProvider => _aiModel.ModelProvider;

        [ObservableProperty]
        private bool _enabled = enabled;

        public long? Size => _aiModel.Size;

        public string? DisplayName => _aiModel.DisplayName;

        public AIModel GetModel() => _aiModel;

        [JsonIgnore]
        public string SizeAsString
        {
            get
            {
                if (Size == null) return string.Empty;

                const long kiloByte = 1024;
                const long megaByte = kiloByte * 1024;
                const long gigaByte = megaByte * 1024;
                const long teraByte = gigaByte * 1024;

                if (Size >= teraByte)
                {
                    return $"{(double)Size / teraByte:F1}TB";
                }
                else if (Size >= gigaByte)
                {
                    return $"{(double)Size / gigaByte:F1}GB";
                }
                else if (Size >= megaByte)
                {
                    return $"{(double)Size / megaByte:F1}MB";
                }
                else if (Size >= kiloByte)
                {
                    return $"{(double)Size / kiloByte:F1}KB";
                }
                else
                {
                    return $"{Size} Bytes";
                }
            }
        }
    }
}