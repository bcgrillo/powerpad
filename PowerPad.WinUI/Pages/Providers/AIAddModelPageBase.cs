using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using PowerPad.WinUI.Components;
using PowerPad.WinUI.ViewModels.AI;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Pages.Providers
{
    public abstract class AIAddModelPageBase(AIModelsViewModelBase aiModelsViewModel) : DisposablePage
    {
        protected SettingsViewModel _settings = App.Get<SettingsViewModel>();
        protected AIModelsViewModelBase _modelsViewModel = aiModelsViewModel;

        protected void Search_Click(object _, RoutedEventArgs __) => Search();

        protected void Search_KeyDown(object _, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter) Search();
        }

        protected virtual void Search() => _modelsViewModel.SearchModelCommand.Execute(GetSearchTextBox().Text);

        protected void AddModel_Click(object _, AIModelClickEventArgs eventArgs)
        {
            // Use Task.Run to offload the work to a background thread and avoid blocking the UI thread.
            Task.Run(() =>
            {
                DispatcherQueue.TryEnqueue(async () =>
                {
                    await _modelsViewModel.AddModelCommand.ExecuteAsync(eventArgs.Model);
                });
            });
        }

        protected abstract TextBox GetSearchTextBox();

        ~AIAddModelPageBase()
        {
            Dispose(false);
        }

        public override void Dispose()
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
