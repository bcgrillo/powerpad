using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace PowerPad.WinUI.Components.Controls
{
    public partial class Label : UserControl
    {
        private Brush? _previousForegroundBrush;

        public string? Text
        {
            get => (string?)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(Label), new PropertyMetadata(null));

        public Label()
        {
            this.InitializeComponent();
        }

        private void Label_IsEnabledChanged(object _, DependencyPropertyChangedEventArgs __) => UpdateForeground();

        private void Label_Loaded(object _, RoutedEventArgs __) => UpdateForeground();

        public void UpdateForeground()
        {
            _previousForegroundBrush ??= TextBlock.Foreground;

            TextBlock.Foreground = IsEnabled 
                ? _previousForegroundBrush
                : (Brush)Application.Current.Resources["TextFillColorDisabledBrush"];
        } 
    }
}
