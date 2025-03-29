using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Models;
using PowerPad.WinUI.ViewModels;
using System;
using Microsoft.UI.Xaml;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using PowerPad.WinUI.ViewModels.AI;

namespace PowerPad.WinUI.Pages.Providers
{
    public sealed partial class OllamaModelsPage : Page
    {
        private readonly OllamaViewModel _ollama;

        public OllamaModelsPage()
        {
            this.InitializeComponent();

            _ollama = new OllamaViewModel();
        }

        private void SetDefault_Click(object sender, RoutedEventArgs __)
        {
            var aiModel = (AIModelViewModel)((Button)sender).DataContext;

            _ollama.SetDefaultModelCommand.Execute(aiModel.GetModel());
        }
    }
}
