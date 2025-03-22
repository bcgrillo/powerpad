using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.ViewModels;
using System;

namespace PowerPad.WinUI.Pages.Providers
{
    public sealed partial class OllamaModelsPage : Page
    {
        private readonly OllamaViewModel _ollama;

        public OllamaModelsPage()
        {
            this.InitializeComponent();

            var aiServicesCollection = Ioc.Default.GetRequiredService<AIServicesVMCollection>();

            _ollama = aiServicesCollection.GetVM<OllamaViewModel>() ?? throw new InvalidOperationException("Ollama View Model not found");
        }
    }
}
