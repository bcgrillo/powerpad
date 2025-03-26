using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Models;
using PowerPad.WinUI.ViewModels;
using System;
using Microsoft.UI.Xaml;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;

namespace PowerPad.WinUI.Pages.Providers
{
    public sealed partial class OllamaModelsPage : Page
    {
        private readonly OllamaViewModel _ollama;

        public OllamaModelsPage()
        {
            this.InitializeComponent();

            _ollama = new OllamaViewModel();

            _ollama.PropertyChanged += Ollama_PropertyChanged;

            UpdateLoadingState();
        }

        private void Ollama_PropertyChanged(object? _, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_ollama.Models)) UpdateLoadingState();
        }

        private void UpdateLoadingState()
        {
            if (_ollama.Models == null)
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
