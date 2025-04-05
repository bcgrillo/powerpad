using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using PowerPad.Core.Models.AI;
using System;
using Microsoft.UI.Xaml;

namespace PowerPad.WinUI.Helpers
{
    public static class ServiceProviderIconResolver
    {
        public static ImageIcon GetIcon(this ModelProvider provider)
        {
            return provider switch
            {
                ModelProvider.Ollama => new() { Source = (ImageSource)Application.Current.Resources["OllamaSvg"] },
                ModelProvider.HuggingFace => new() { Source = (ImageSource)Application.Current.Resources["HuggingFaceSvg"] },
                ModelProvider.GitHub => new() { Source = (ImageSource)Application.Current.Resources["GithubSvg"] },
                ModelProvider.OpenAI => new() { Source = (ImageSource)Application.Current.Resources["OpenAISvg"] },
                _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
            };
        }
    }
}
