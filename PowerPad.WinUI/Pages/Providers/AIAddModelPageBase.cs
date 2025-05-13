using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using PowerPad.WinUI.Components;
using PowerPad.WinUI.ViewModels.AI.Providers;
using PowerPad.WinUI.ViewModels.Settings;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Pages.Providers
{
    /// <summary>
    /// Base class for pages that allow adding AI models. Provides common functionality for searching and adding models.
    /// </summary>
    /// <param name="aiModelsViewModel">The <see cref="AIModelsViewModelBase"/> for managing AI models.</param>
    public abstract class AIAddModelPageBase(AIModelsViewModelBase aiModelsViewModel) : DisposablePage, IModelProviderPage
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
        /// Executes the search command using the text from the search text box.
        /// </summary>
        public virtual void Search() => _modelsViewModel.SearchModelCommand.Execute(GetSearchTextBox().Text);

        /// <inheritdoc />
        public abstract void CloseModelInfoViewer();

        /// <summary>
        /// Handles the click event for the search button.
        /// </summary>
        protected void Search_Click(object _, RoutedEventArgs __) => Search();

        /// <summary>
        /// Handles the key down event for the search text box.
        /// Executes the search when the Enter key is pressed.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="eventArgs">The key event arguments.</param>
        protected void Search_KeyDown(object _, KeyRoutedEventArgs eventArgs)
        {
            if (eventArgs.Key == Windows.System.VirtualKey.Enter) Search();
        }

        /// <summary>
        /// Handles the click event for adding a model.
        /// Executes the add model command asynchronously.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="eventArgs">The event arguments containing the model to add.</param>
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

        /// <summary>
        /// Gets the search <see cref="TextBox"/> control.
        /// </summary>
        /// <returns>The <see cref="TextBox"/> used for search input.</returns>
        protected abstract TextBox GetSearchTextBox();

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing) _modelsViewModel.Dispose();
        }
    }
}
