using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;

namespace PowerPad.WinUI.Components.Controls
{
    public class IntegratedTextBox : TextBox
    {
        public Brush? ForcedForeground
        {
            get => (Brush?)GetValue(ForcedForegroundProperty);
            set => SetValue(ForcedForegroundProperty, value);
        }

        public static readonly DependencyProperty ForcedForegroundProperty =
            DependencyProperty.Register(nameof(ForcedForeground), typeof(Brush), typeof(IntegratedTextBox), new(null));

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
        }

        private void UpdateForeground(DependencyObject sender, DependencyProperty dp)
        {
            var foregroundBrush = IsEnabled
                ?  (IsReadOnly
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