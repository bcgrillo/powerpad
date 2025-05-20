using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.Components.Controls;
using PowerPad.WinUI.ViewModels.AI;
using System;
using System.Collections.ObjectModel;

namespace PowerPad.WinUI.Components
{
    /// <summary>
    /// Represents a UI control that displays a list of available AI models and provides interaction options.
    /// </summary>
    public partial class AvailableModelsRepeater : UserControl
    {
        /// <summary>
        /// Gets or sets the collection of AI models to display.
        /// </summary>
        public ObservableCollection<AIModelViewModel> Models
        {
            get => (ObservableCollection<AIModelViewModel>)GetValue(ModelsProperty);
            set => SetValue(ModelsProperty, value);
        }

        /// <summary>
        /// Dependency property for the <see cref="Models"/> property.
        /// </summary>
        public static readonly DependencyProperty ModelsProperty =
            DependencyProperty.Register(nameof(Models), typeof(ObservableCollection<AIModelViewModel>), typeof(AvailableModelsRepeater), new(null));

        /// <summary>
        /// Gets or sets a value indicating whether the models collection is empty.
        /// </summary>
        public bool ModelsEmpty
        {
            get => (bool)GetValue(ModelsEmptyProperty);
            set => SetValue(ModelsEmptyProperty, value);
        }

        /// <summary>
        /// Dependency property for the <see cref="ModelsEmpty"/> property.
        /// </summary>
        public static readonly DependencyProperty ModelsEmptyProperty =
            DependencyProperty.Register(nameof(ModelsEmpty), typeof(bool), typeof(AvailableModelsRepeater), new(false));

        /// <summary>
        /// Occurs when the delete button is clicked for a model.
        /// </summary>
        public event EventHandler<AIModelClickEventArgs>? DeleteClick;

        /// <summary>
        /// Occurs when the set default button is clicked for a model.
        /// </summary>
        public event EventHandler<AIModelClickEventArgs>? SetDefaultClick;

        /// <summary>
        /// Occurs when the add button is clicked.
        /// </summary>
        public event EventHandler? AddButtonClick;

        /// <summary>
        /// Occurs when the visibility of the model info viewer changes.
        /// </summary>
        public event EventHandler<ModelInfoViewerVisibilityEventArgs>? ModelInfoViewerVisibilityChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvailableModelsRepeater"/> class.
        /// </summary>
        public AvailableModelsRepeater()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Closes the model info viewer.
        /// </summary>
        public void CloseModelInfoViewer() => ModelInfoViewer.Hide();

        /// <summary>
        /// Handles the delete button click event for a model.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="__">The event arguments (not used).</param>
        private void OnDeleteClick(object? sender, RoutedEventArgs __)
        {
            DeleteClick?.Invoke(sender, new((AIModelViewModel)((MenuFlyoutItem)sender!).Tag));
        }

        /// <summary>
        /// Handles the set default button click event for a model.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="__">The event arguments (not used).</param>
        private void OnSetDefaultClick(object? sender, RoutedEventArgs __)
        {
            var model = (AIModelViewModel)((MenuFlyoutItem)sender!).Tag;

            if (!model.Enabled) model.Enabled = true;

            SetDefaultClick?.Invoke(sender, new(model));
        }

        /// <summary>
        /// Handles the hyperlink button click event to show model information.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="__">The event arguments (not used).</param>
        private void HyperlinkButton_Click(object sender, RoutedEventArgs __)
        {
            var model = (AIModelViewModel)((HyperlinkButton)sender!).Tag;

            ModelInfoViewer.Show(model.CardName, model.InfoUrl!);
        }

        /// <summary>
        /// Handles the visibility change event of the model info viewer.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="eventArgs">The event arguments containing visibility information.</param>
        private void ModelInfoViewer_VisibilityChanged(object sender, ModelInfoViewerVisibilityEventArgs eventArgs)
        {
            ModelsScrollViewer.Visibility = eventArgs.IsVisible ? Visibility.Collapsed : Visibility.Visible;
            ModelInfoViewerVisibilityChanged?.Invoke(sender, eventArgs);
        }

        /// <summary>
        /// Handles the add models button click event.
        /// </summary>
        private void AddModelsButton_Click(object _, RoutedEventArgs __)
        {
            AddButtonClick?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Provides data for events related to AI model interactions.
    /// </summary>
    public class AIModelClickEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AIModelClickEventArgs"/> class.
        /// </summary>
        /// <param name="model">The AI model associated with the event.</param>
        public AIModelClickEventArgs(AIModelViewModel model)
        {
            Model = model;
        }

        /// <summary>
        /// Gets the AI model associated with the event.
        /// </summary>
        public AIModelViewModel Model { get; }
    }
}
