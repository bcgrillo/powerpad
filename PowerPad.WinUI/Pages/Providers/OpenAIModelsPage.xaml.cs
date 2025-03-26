using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.ViewModels;
using System;
using Microsoft.UI.Xaml;
using System.ComponentModel;

namespace PowerPad.WinUI.Pages.Providers
{
    public sealed partial class OpenAIModelsPage : Page
    {
        private readonly OpenAIViewModel _openAI;

        public OpenAIModelsPage()
        {
            this.InitializeComponent();

            var aiServicesCollection = Ioc.Default.GetRequiredService<SettingsViewModel>();

            _openAI = aiServicesCollection.OpenAI;

            _openAI.PropertyChanged += OpenAI_PropertyChanged;

            UpdateLoadingState();
        }

        private void OpenAI_PropertyChanged(object? _, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_openAI.Models)) UpdateLoadingState();
        }

        private void UpdateLoadingState()
        {
            if (_openAI.Models == null)
            {
                LoadingIndicator.Visibility = Visibility.Visible;
                LoadingText.Visibility = Visibility.Visible;
                ModelsScrollViewer.Visibility = Visibility.Collapsed;
            }
            else
            {
                LoadingIndicator.Visibility = Visibility.Collapsed;
                LoadingText.Visibility = Visibility.Collapsed;
                ModelsScrollViewer.Visibility = Visibility.Visible;
            }
        }
    }
}
