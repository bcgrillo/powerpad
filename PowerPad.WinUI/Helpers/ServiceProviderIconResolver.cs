using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using PowerPad.Core.Models.AI;
using System;

namespace PowerPad.WinUI.Helpers
{
    /// <summary>
    /// Provides helper methods to resolve icons for different AI model providers.
    /// </summary>
    public static class ServiceProviderIconResolver
    {
        /// <summary>
        /// Retrieves the corresponding icon for the specified AI model provider.
        /// </summary>
        /// <param name="provider">The AI model provider for which the icon is to be resolved.</param>
        /// <returns>An <see cref="ImageSource"/> representing the icon for the specified provider.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the specified provider is not recognized.</exception>
        public static ImageSource GetIcon(this ModelProvider provider)
        {
            return provider switch
            {
                ModelProvider.Ollama => (ImageSource)Application.Current.Resources["OllamaSvg"],
                ModelProvider.HuggingFace => (ImageSource)Application.Current.Resources["HuggingFaceSvg"],
                ModelProvider.GitHub => (ImageSource)Application.Current.Resources["GitHubSvg"],
                ModelProvider.OpenAI => (ImageSource)Application.Current.Resources["OpenAISvg"],
                _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
            };
        }
    }
}
