using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.ViewModels;
using System;

namespace PowerPad.WinUI.Pages.AIServices
{
    public sealed partial class OllamaPage : Page
    {
        private readonly OllamaViewModel _ollama;

        public OllamaPage()
        {
            this.InitializeComponent();

            var aiServicesCollection = Ioc.Default.GetRequiredService<AIServicesVMCollection>();

            _ollama = aiServicesCollection.GetVM<OllamaViewModel>() ?? throw new InvalidOperationException("Ollama View Model not found");
        }
    }
}
