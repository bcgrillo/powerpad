using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;

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

            IsEnabledChanged += (s, e) => UpdateEnabledLayout((bool)e.NewValue);

            //TODO: Check if is possible registes a new callback to IsEnabledProperty, isEnebledChanged is not fired
            //when the change of IsEnabled come from the parent control "IsEnabled" change
        }

        public void UpdateEnabledLayout(bool newValue) => Opacity = newValue ? ENABLED_OPACITY : DISABLED_OPACITY;
    }
}