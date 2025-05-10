using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using PowerPad.WinUI.Components;
using PowerPad.WinUI.ViewModels.AI.Providers;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Pages.Providers
{
    public abstract class AIAddModelPageBase(AIModelsViewModelBase aiModelsViewModel) : DisposablePage, IModelProviderPage
    {
        protected SettingsViewModel _settings = App.Get<SettingsViewModel>();
        protected AIModelsViewModelBase _modelsViewModel = aiModelsViewModel;

        protected void Search_Click(object _, RoutedEventArgs __) => Search();

        protected void Search_KeyDown(object _, KeyRoutedEventArgs eventArgs)
        {
            if (eventArgs.Key == Windows.System.VirtualKey.Enter) Search();
        }

        public virtual void Search() => _modelsViewModel.SearchModelCommand.Execute(GetSearchTextBox().Text);

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

        protected override void Dispose(bool disposing)
        {
            if (disposing) _modelsViewModel.Dispose();
        }

        public abstract void CloseModelInfoViewer();
    }
}
