using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace PowerPad.WinUI.Components.Controls
{
    /// <summary>
    /// A custom TextBox control with additional features such as configurable foreground color
    /// and automatic foreground updates based on the control's state.
    /// </summary>
    public class IntegratedTextBox : TextBox
    {
        /// <summary>
        /// Gets or sets the forced foreground brush for the control.
        /// If set, this brush will override the default foreground behavior.
        /// </summary>
        public Brush? ForcedForeground
        {
            get => (Brush?)GetValue(ForcedForegroundProperty);
            set => SetValue(ForcedForegroundProperty, value);
        }

        /// <summary>
        /// Dependency property for <see cref="ForcedForeground"/>.
        /// </summary>
        public static readonly DependencyProperty ForcedForegroundProperty =
            DependencyProperty.Register(nameof(ForcedForeground), typeof(Brush), typeof(IntegratedTextBox), new(null));

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegratedTextBox"/> class.
        /// Configures default styles and registers property change callbacks.
        /// </summary>
        public IntegratedTextBox()
        {
            DefaultStyleKey = typeof(IntegratedTextBox);

            Resources["TextControlBorderThemeThickness"] = new Thickness(0);
            Resources["TextControlBorderThemeThicknessFocused"] = new Thickness(0);
            Resources["TextControlBackground"] = new SolidColorBrush(Colors.Transparent);
            Resources["TextControlBackgroundPointerOver"] = new SolidColorBrush(Colors.Transparent);
            Resources["TextControlBackgroundFocused"] = new SolidColorBrush(Colors.Transparent);
            Resources["TextControlBackgroundDisabled"] = new SolidColorBrush(Colors.Transparent);

            RegisterPropertyChangedCallback(TextBox.IsReadOnlyProperty, UpdateForeground);
            RegisterPropertyChangedCallback(TextBox.IsEnabledProperty, UpdateForeground);
            RegisterPropertyChangedCallback(IntegratedTextBox.ForcedForegroundProperty, UpdateForeground);

            IsSpellCheckEnabled = false;
        }

        /// <summary>
        /// Updates the foreground color of the control based on its state (enabled, read-only, etc.).
        /// </summary>
        /// <param name="sender">The dependency object that triggered the callback.</param>
        /// <param name="dp">The dependency property that changed.</param>
        private void UpdateForeground(DependencyObject sender, DependencyProperty dp)
        {
            var foregroundBrush = IsEnabled
                ? (IsReadOnly
                    ? (ForcedForeground ?? (Brush)Application.Current.Resources["TextFillColorTertiaryBrush"])
                    : (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"])
                : (Brush)Application.Current.Resources["TextFillColorDisabledBrush"];

            Foreground = foregroundBrush;
            Resources["TextControlForeground"] = foregroundBrush;
            Resources["TextControlForegroundPointerOver"] = foregroundBrush;
            Resources["TextControlForegroundFocused"] = foregroundBrush;
            Resources["TextControlForegroundDisabled"] = foregroundBrush;
        }
    }
}