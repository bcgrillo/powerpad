using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;
using Microsoft.UI.Xaml;
using System.ComponentModel;
using PowerPad.WinUI.ViewModels.AI;

namespace PowerPad.WinUI.Pages.Providers
{
    public sealed partial class AzureAIModelsPage : Page
    {
        private readonly AzureAIViewModel _azureAI;

        public AzureAIModelsPage()
        {
            this.InitializeComponent();

            _azureAI = new AzureAIViewModel();
        }

        private void SetDefault_Click(object sender, RoutedEventArgs __)
        {
            var aiModel = (AIModelViewModel)((Button)sender).DataContext;

            _azureAI.SetDefaultModelCommand.Execute(aiModel.GetModel());
        }
    }
}
