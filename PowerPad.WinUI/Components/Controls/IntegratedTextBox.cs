using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;

namespace PowerPad.WinUI.Components.Controls
{
    public class IntegratedTextBox : TextBox
    {
        public IntegratedTextBox()
        {
            this.DefaultStyleKey = typeof(IntegratedTextBox);

            this.Resources["TextControlBorderThemeThickness"] = new Thickness(0);
            this.Resources["TextControlBorderThemeThicknessFocused"] = new Thickness(0);
            this.Resources["TextControlBackground"] = new SolidColorBrush(Colors.Transparent);
            this.Resources["TextControlBackgroundPointerOver"] = new SolidColorBrush(Colors.Transparent);
            this.Resources["TextControlBackgroundFocused"] = new SolidColorBrush(Colors.Transparent);
            this.Resources["TextControlBackgroundDisabled"] = new SolidColorBrush(Colors.Transparent);
            
            this.RegisterPropertyChangedCallback(TextBox.IsReadOnlyProperty, UpdateForeground);
            this.RegisterPropertyChangedCallback(TextBox.IsEnabledProperty, UpdateForeground);
        }

        private void UpdateForeground(DependencyObject sender, DependencyProperty dp)
        {
            var foregroundBrush = IsEnabled
                ?  (IsReadOnly
                    ? (Brush)Application.Current.Resources["TextFillColorTertiaryBrush"]
                    : (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"])
                : (Brush)Application.Current.Resources["TextFillColorDisabledBrush"];

            this.Foreground = foregroundBrush;
            this.Resources["TextControlForeground"] = foregroundBrush;
            this.Resources["TextControlForegroundPointerOver"] = foregroundBrush;
            this.Resources["TextControlForegroundFocused"] = foregroundBrush;
            this.Resources["TextControlForegroundDisabled"] = foregroundBrush;
        }
    }
}