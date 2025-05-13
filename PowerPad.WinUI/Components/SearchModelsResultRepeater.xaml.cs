using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.Components.Controls;
using PowerPad.WinUI.ViewModels.AI;
using System;
using System.Collections.ObjectModel;

namespace PowerPad.WinUI.Components
{
    /// <summary>
    /// Represents a user control that displays a list of AI models and handles interactions with them.
    /// </summary>
    public partial class SearchModelsResultRepeater : UserControl
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
        /// Dependency property for <see cref="Models"/>.
        /// </summary>
        public static readonly DependencyProperty ModelsProperty =
            DependencyProperty.Register(nameof(Models), typeof(ObservableCollection<AIModelViewModel>), typeof(AvailableModelsRepeater), new(null));

        /// <summary>
        /// Gets or sets a value indicating whether a search operation is in progress.
        /// </summary>
        public bool SearchingFlag
        {
            get => (bool)GetValue(SearchingFlagProperty);
            set => SetValue(SearchingFlagProperty, value);
        }

        /// <summary>
        /// Dependency property for <see cref="SearchingFlag"/>.
        /// </summary>
        public static readonly DependencyProperty SearchingFlagProperty =
            DependencyProperty.Register(nameof(SearchingFlag), typeof(bool), typeof(SearchModelsResultRepeater), new(false));

        /// <summary>
        /// Gets or sets a value indicating whether the search result is empty.
        /// </summary>
        public bool SearchEmpty
        {
            get => (bool)GetValue(SearchEmptyProperty);
            set => SetValue(SearchEmptyProperty, value);
        }

        /// <summary>
        /// Dependency property for <see cref="SearchEmpty"/>.
        /// </summary>
        public static readonly DependencyProperty SearchEmptyProperty =
            DependencyProperty.Register(nameof(SearchEmpty), typeof(bool), typeof(AvailableModelsRepeater), new(false));

        /// <summary>
        /// Occurs when the "Add Model" button is clicked.
        /// </summary>
        public event EventHandler<AIModelClickEventArgs>? AddModelClick;

        /// <summary>
        /// Occurs when the visibility of the model info viewer changes.
        /// </summary>
        public event EventHandler<ModelInfoViewerVisibilityEventArgs>? ModelInfoViewerVisibilityChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchModelsResultRepeater"/> class.
        /// </summary>
        public SearchModelsResultRepeater()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Closes the model info viewer.
        /// </summary>
        public void CloseModelInfoViewer() => ModelInfoViewer.Hide();

        /// <summary>
        /// Handles the "Add Model" button click event.
        /// </summary>
        /// <param name="sender">The source of the event, typically a <see cref="Button"/>.</param>
        /// <param name="__">The event arguments (not used).</param>
        private void OnAddModelClick(object? sender, RoutedEventArgs __)
        {
            AddModelClick?.Invoke(sender, new((AIModelViewModel)((Button)sender!).Tag));
        }

        /// <summary>
        /// Handles the hyperlink button click event to show model information.
        /// </summary>
        /// <param name="sender">The source of the event, typically a <see cref="HyperlinkButton"/>.</param>
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
        /// <param name="eventArgs">The event arguments containing visibility details.</param>
        private void ModelInfoViewer_VisibilityChanged(object sender, ModelInfoViewerVisibilityEventArgs eventArgs)
        {
            MainContent.Visibility = eventArgs.IsVisible ? Visibility.Collapsed : Visibility.Visible;
            ModelInfoViewerVisibilityChanged?.Invoke(sender, eventArgs);
        }
    }
}