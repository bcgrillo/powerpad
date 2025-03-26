using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using PowerPad.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.WinUI.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        public required ObservableCollection<AIServiceViewModel> _services;

        [ObservableProperty]
        private OllamaViewModel _ollama;

        [ObservableProperty]
        private AzureAIViewModel _azureAI;

        [ObservableProperty]
        private OpenAIViewModel _openAI;

        public SettingsViewModel()
        {
            Ollama = new OllamaViewModel(Ioc.Default.GetRequiredService<IOllamaService>());
            AzureAI = new AzureAIViewModel(Ioc.Default.GetRequiredService<IAzureAIService>());
            OpenAI = new OpenAIViewModel(Ioc.Default.GetRequiredService<IOpenAIService>());

            Services = [Ollama, AzureAI, OpenAI];
        }
    }
}
