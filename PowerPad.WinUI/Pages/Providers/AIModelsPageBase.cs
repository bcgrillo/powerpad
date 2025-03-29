using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.Components;
using PowerPad.WinUI.ViewModels.AI;
using System;

namespace PowerPad.WinUI.Pages.Providers
{
    public abstract class AIModelsPageBase : Page
    {
        protected AIModelsViewModelBase _modelsViewModel = null!;

        protected void SetDefault_Click(object _, AIModelClickEventArgs eventArgs)
        {
            _modelsViewModel.SetDefaultModelCommand.Execute(eventArgs.Model);
        }

        protected void Delete_Click(object _, AIModelClickEventArgs eventArgs)
        {
            _modelsViewModel.RemoveModelCommand.Execute(eventArgs.Model);
        }
    }
}
