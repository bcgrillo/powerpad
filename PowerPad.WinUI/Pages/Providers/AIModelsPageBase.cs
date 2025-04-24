using PowerPad.WinUI.Components;
using PowerPad.WinUI.ViewModels.AI.Providers;
using PowerPad.WinUI.ViewModels.Settings;
using System;

namespace PowerPad.WinUI.Pages.Providers
{
    public abstract class AIModelsPageBase(AIModelsViewModelBase aiModelsViewModel) : DisposablePage, IModelProviderPage
    {
        protected SettingsViewModel _settings = App.Get<SettingsViewModel>();
        protected AIModelsViewModelBase _modelsViewModel = aiModelsViewModel;

        public event EventHandler? AddButtonClick;

        protected void SetDefault_Click(object _, AIModelClickEventArgs eventArgs)
        {
            _modelsViewModel.SetDefaultModelCommand.Execute(eventArgs.Model);
        }

        protected void Delete_Click(object _, AIModelClickEventArgs eventArgs)
        {
            _modelsViewModel.RemoveModelCommand.Execute(eventArgs.Model);
        }

        protected void AvailableModelsRepeater_AddButtonClick(object sender, System.EventArgs e) => AddButtonClick?.Invoke(sender, e);

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

        public abstract void CloseModelInfoViewer();
    }
}
