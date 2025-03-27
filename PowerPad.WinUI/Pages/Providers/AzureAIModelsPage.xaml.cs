using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.ViewModels;
using System;
using Microsoft.UI.Xaml;
using System.ComponentModel;

namespace PowerPad.WinUI.Pages.Providers
{
    public sealed partial class AzureAIModelsPage : Page
    {
        private readonly AzureAIViewModel _azureAI;

        public AzureAIModelsPage()
        {
            this.InitializeComponent();

            _azureAI = new AzureAIViewModel();

            _azureAI.PropertyChanged += OpenAI_PropertyChanged;

            UpdateLoadingState();
        }

        private void OpenAI_PropertyChanged(object? _, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_azureAI.Models)) UpdateLoadingState();
        }

        private void UpdateLoadingState()
        {
            if (_azureAI.Models == null)
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

        private void SetDefault_Click(object sender, RoutedEventArgs e)
        {
            _azureAI.
        }
    }
}
