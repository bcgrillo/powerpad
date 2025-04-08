using Microsoft.UI.Xaml.Media;
using PowerPad.Core.Models.AI;
using System;
using Microsoft.UI.Xaml;

namespace PowerPad.WinUI.Helpers
{
    public static class ServiceProviderIconResolver
    {
        public static ImageSource GetIcon(this ModelProvider provider)
        {
            return provider switch
            {
                ModelProvider.Ollama => (ImageSource)Application.Current.Resources["OllamaSvg"],
                ModelProvider.HuggingFace => (ImageSource)Application.Current.Resources["HuggingFaceSvg"],
                ModelProvider.GitHub => (ImageSource)Application.Current.Resources["GithubSvg"],
                ModelProvider.OpenAI => (ImageSource)Application.Current.Resources["OpenAISvg"],
                _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
            };
        }
    }
}
