using PowerPad.WinUI.Components;
using PowerPad.WinUI.ViewModels.AI.Providers;
using PowerPad.WinUI.ViewModels.Settings;
using System;

namespace PowerPad.WinUI.Pages.Providers
{
    /// <summary>
    /// Base class for pages that manage AI models. Provides common functionality for handling AI models.
    /// </summary>
    /// <param name="aiModelsViewModel">The <see cref="AIModelsViewModelBase"/> for managing AI models.</param>
    public abstract class AIModelsPageBase(AIModelsViewModelBase aiModelsViewModel) : DisposablePage, IModelProviderPage
    {
        /// <summary>
        /// The <see cref="SettingsViewModel"/> for application-wide settings.
        /// </summary>
        protected SettingsViewModel _settings = App.Get<SettingsViewModel>();

        /// <summary>
        /// The <see cref="AIModelsViewModelBase"/> for managing AI models.
        /// </summary>
        protected AIModelsViewModelBase _modelsViewModel = aiModelsViewModel;

        /// <summary>
        /// Event triggered when the Add button is clicked.
        /// </summary>
        public event EventHandler? AddButtonClick;

        /// <inheritdoc />
        public abstract void CloseModelInfoViewer();

        /// <summary>
        /// Handles the click event to set the default AI model.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="eventArgs">Event arguments containing the AI model to set as default.</param>
        protected void SetDefault_Click(object _, AIModelClickEventArgs eventArgs)
        {
            _modelsViewModel.SetDefaultModelCommand.Execute(eventArgs.Model);
        }

        /// <summary>
        /// Handles the click event to delete an AI model.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="eventArgs">Event arguments containing the AI model to delete.</param>
        protected void Delete_Click(object _, AIModelClickEventArgs eventArgs)
        {
            _modelsViewModel.RemoveModelCommand.Execute(eventArgs.Model);
        }

        /// <summary>
        /// Handles the Add button click event in the Available Models Repeater.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">Event arguments for the Add button click.</param>
        protected void AvailableModelsRepeater_AddButtonClick(object sender, EventArgs eventArgs) => AddButtonClick?.Invoke(sender, eventArgs);

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing) _modelsViewModel.Dispose();
        }
    }
}
