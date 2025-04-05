using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using PowerPad.WinUI.Components;
using PowerPad.WinUI.ViewModels.AI;
using System;

namespace PowerPad.WinUI.Pages.Providers
{
    public abstract class AIAddModelPageBase(AIModelsViewModelBase aIModelsViewModel) : Page, IDisposable
    {
        private bool _disposed;

        protected AIModelsViewModelBase _modelsViewModel = aIModelsViewModel;

        protected void Search_Click(object _, RoutedEventArgs __)
        {
            _modelsViewModel.SearchModelCommand.Execute(GetSearchTextBox().Text);
        }

        protected void Search_KeyDown(object _, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                _modelsViewModel.SearchModelCommand.Execute(GetSearchTextBox().Text);
            }
        }

        protected void AddModel_Click(object _, AIModelClickEventArgs eventArgs)
        {
            _modelsViewModel.AddModelCommand.Execute(eventArgs.Model);
        }

        protected abstract TextBox GetSearchTextBox();

        ~AIAddModelPageBase()
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
