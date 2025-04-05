using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.Components;
using PowerPad.WinUI.ViewModels.AI;
using System;

namespace PowerPad.WinUI.Pages.Providers
{
    public abstract class AIModelsPageBase(AIModelsViewModelBase aIModelsViewModel) : Page, IDisposable
    {
        private bool _disposed;

        protected AIModelsViewModelBase _modelsViewModel = aIModelsViewModel;

        protected void SetDefault_Click(object _, AIModelClickEventArgs eventArgs)
        {
            _modelsViewModel.SetDefaultModelCommand.Execute(eventArgs.Model);
        }

        protected void Delete_Click(object _, AIModelClickEventArgs eventArgs)
        {
            _modelsViewModel.RemoveModelCommand.Execute(eventArgs.Model);
        }

        ~AIModelsPageBase()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing) _modelsViewModel.Dispose();

                _disposed = true;
            }
        }
    }
}
