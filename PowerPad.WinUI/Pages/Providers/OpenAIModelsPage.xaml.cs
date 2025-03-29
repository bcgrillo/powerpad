using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;
using Microsoft.UI.Xaml;
using System.ComponentModel;
using PowerPad.WinUI.ViewModels.AI;

namespace PowerPad.WinUI.Pages.Providers
{
    public sealed partial class OpenAIModelsPage : Page
    {
        private readonly OpenAIViewModel _openAI;

        public OpenAIModelsPage()
        {
            this.InitializeComponent();

            _openAI = new OpenAIViewModel();
        }

        private void SetDefault_Click(object sender, RoutedEventArgs __)
        {
            var aiModel = (AIModelViewModel)((Button)sender).DataContext;

            _openAI.SetDefaultModelCommand.Execute(aiModel.GetModel());
        }
    }
}
