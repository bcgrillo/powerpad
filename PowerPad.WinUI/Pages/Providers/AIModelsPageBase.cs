using PowerPad.WinUI.Components;
using PowerPad.WinUI.ViewModels.AI.Providers;
using PowerPad.WinUI.ViewModels.Settings;
using System;

namespace PowerPad.WinUI.Pages.Providers
{
    public abstract class AIModelsPageBase(AIModelsViewModelBase aiModelsViewModel) : DisposablePage
    {
        protected SettingsViewModel _settings = App.Get<SettingsViewModel>();
        protected AIModelsViewModelBase _modelsViewModel = aiModelsViewModel;

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
