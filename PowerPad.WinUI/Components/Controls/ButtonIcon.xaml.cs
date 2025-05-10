using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace PowerPad.WinUI.Components.Controls
{
    /// <summary>
    /// Represents a custom button control with an icon and adjustable opacity based on its enabled state.
    /// </summary>
    public partial class ButtonIcon : UserControl
    {
        // Opacity values for enabled and disabled states
        private readonly float ENABLED_OPACITY = 0.9f;
        private readonly float DISABLED_OPACITY = 0.5f;

        /// <summary>
        /// Gets or sets the image source for the button icon.
        /// </summary>
        public ImageSource? Source
        {
            get => (ImageSource?)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        /// <summary>
        /// Dependency property for the <see cref="Source"/> property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(ImageSource), typeof(ButtonIcon), new(null));

        /// <summary>
        /// Initializes a new instance of the <see cref="ButtonIcon"/> class.
        /// </summary>
        public ButtonIcon()
        {
            this.InitializeComponent();

            // Subscribe to the IsEnabledChanged event to update the layout when the enabled state changes.
            IsEnabledChanged += (s, e) => UpdateEnabledLayout((bool)e.NewValue);

            // TODO: Check if it is possible to register a new callback to the IsEnabledProperty.
            // The IsEnabledChanged event is not fired when the change of IsEnabled comes from the parent control's "IsEnabled" change.
        }

        /// <summary>
        /// Updates the layout of the button based on its enabled state.
        /// </summary>
        /// <param name="newValue">The new enabled state of the button.</param>
        public void UpdateEnabledLayout(bool newValue) => Opacity = newValue ? ENABLED_OPACITY : DISABLED_OPACITY;
    }
}