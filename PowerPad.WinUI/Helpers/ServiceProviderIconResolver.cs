using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using PowerPad.Core.Models.AI;
using System;
using Microsoft.UI.Xaml;

namespace PowerPad.WinUI.Helpers
{
    public static class ServiceProviderIconResolver
    {
        public static IconElement GetIcon(this ModelProvider provider)
        {
            return provider switch
            {
                ModelProvider.Ollama => new ImageIcon { Source = (ImageSource)Application.Current.Resources["OllamaSvg"] },
                ModelProvider.HuggingFace => new ImageIcon { Source = (ImageSource)Application.Current.Resources["HuggingFaceSvg"] },
                ModelProvider.GitHub => new ImageIcon { Source = (ImageSource)Application.Current.Resources["GithubSvg"] },
                ModelProvider.OpenAI => new ImageIcon { Source = (ImageSource)Application.Current.Resources["OpenAISvg"] },
                _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
            };
        }
    }
}
