using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace PowerPad.WinUI.Components.Controls
{
    public partial class ButtonIcon : UserControl
    {
        private readonly float ENABLED_OPACITY = 0.9f;
        private readonly float DISABLED_OPACITY = 0.5f;

        public ImageSource? Source
        {
            get => (ImageSource?)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(ImageSource), typeof(ButtonIcon), new(null));

        public ButtonIcon()
        {
            this.InitializeComponent();

            IsEnabledChanged += (s, e) => Opacity = IsEnabled ? ENABLED_OPACITY : DISABLED_OPACITY;
        }
    }
}
