using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace PowerPad.WinUI.Components.Controls
{
    public sealed partial class ButtonIcon : UserControl
    {
        private readonly float ENABLED_OPACITY = 0.9f;
        private readonly float DISABLED_OPACITY = 0.5f;

        public ImageSource? Source
        {
            get => (ImageSource?)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(ImageSource), typeof(ButtonIcon), new(false));

        public ButtonIcon()
        {
            this.InitializeComponent();

            IsEnabledChanged += (s, e) => Opacity = IsEnabled ? ENABLED_OPACITY : DISABLED_OPACITY;
        }
    }
}
