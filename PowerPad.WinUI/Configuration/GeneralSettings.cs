using Azure.AI.Inference;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using PowerPad.Core.Models;
using PowerPad.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Configuration
{
    public partial class GeneralSettings : ObservableObject
    {
        [ObservableProperty]
        private bool _ollamaEnabled;

        [ObservableProperty]
        private bool _azureAIEnabled;

        [ObservableProperty]
        private bool _openAIEnabled;


        [ObservableProperty]
        private AIServiceConfig? _ollamaConfig;

        [ObservableProperty]
        private AIServiceConfig? _azureAIConfig;

        [ObservableProperty]
        private AIServiceConfig? _openAIConfig;


        [ObservableProperty]
        private ApplicationTheme? _appTheme;

        [ObservableProperty]
        private bool _acrylicBackground;
    };
}