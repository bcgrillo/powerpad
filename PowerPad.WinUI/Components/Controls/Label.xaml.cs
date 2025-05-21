using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace PowerPad.WinUI.Components.Controls
{
    /// <summary>
    /// Represents a custom Label control with support for dynamic foreground updates based on the control's enabled state.
    /// </summary>
    public partial class Label : UserControl
    {
        private Brush? _previousForegroundBrush;

        /// <summary>
        /// Gets or sets the text content of the Label.
        /// </summary>
        public string? Text
        {
            get => (string?)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        /// <summary>
        /// DependencyProperty for the Text property.
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(Label), new PropertyMetadata(null));

        /// <summary>
        /// Initializes a new instance of the <see cref="Label"/> class.
        /// </summary>
        public Label()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles the IsEnabledChanged event to update the foreground color.
        /// </summary>
        private void Label_IsEnabledChanged(object _, DependencyPropertyChangedEventArgs __) => UpdateForeground();

        /// <summary>
        /// Handles the Loaded event to update the foreground color.
        /// </summary>
        private void Label_Loaded(object _, RoutedEventArgs __) => UpdateForeground();

        /// <summary>
        /// Updates the foreground color of the Label based on its enabled state.
        /// </summary>
        private void UpdateForeground()
        {
            _previousForegroundBrush ??= TextBlock.Foreground;

            TextBlock.Foreground = IsEnabled
                ? _previousForegroundBrush
                : (Brush)Application.Current.Resources["TextFillColorDisabledBrush"];
        }
    }
}