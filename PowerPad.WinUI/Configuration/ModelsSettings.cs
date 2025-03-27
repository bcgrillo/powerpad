using Azure.AI.Inference;
using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Models;
using PowerPad.Core.Services;
using System.Collections.ObjectModel;

namespace PowerPad.WinUI.Configuration
{
    public partial class ModelsSettings : ObservableObject
    {
        [ObservableProperty]
        private AIModel? _defaultModel;

        [ObservableProperty]
        private AIParameters? _defaultParameters;

        public required ObservableCollection<AIModel> AvailableModels;
    }
}