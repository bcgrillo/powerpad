using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using PowerPad.WinUI.Helpers;
using PowerPad.WinUI.ViewModels.Agents;

namespace PowerPad.WinUI.Components.Controls
{
    public partial class AgentIconControl : UserControl
    {
        public AgentIcon AgentIcon
        {
            get => (AgentIcon)GetValue(AgentIconProperty);
            set => SetValue(AgentIconProperty, value);
        }

        public static readonly DependencyProperty AgentIconProperty =
            DependencyProperty.Register(nameof(AgentIconProperty), typeof(AgentIcon), typeof(AgentIconControl), new(default));

        public double Size
        {
            get => (double)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register(nameof(SizeProperty), typeof(double), typeof(AgentIconControl), new(20));


        public AgentIconControl()
        {
            this.InitializeComponent();

            RegisterPropertyChangedCallback(AgentIconProperty, AgentIconChanged);
            RegisterPropertyChangedCallback(SizeProperty, IconSizeChanged);
        }

        private void AgentIconChanged(DependencyObject _, DependencyProperty __)
        {
            ImageIcon.Visibility = Visibility.Collapsed;
            FontIcon.Visibility = Visibility.Collapsed;

            switch (AgentIcon.Type)
            {
                case AgentIconType.Base64Image:
                    ImageIcon.Source = Base64ImageHelper.LoadImageFromBase64(AgentIcon.Source, Size);
                    ImageIcon.Visibility = Visibility.Visible;
                    break;
                case AgentIconType.FontIconGlyph:
                    FontIcon.Glyph = AgentIcon.Source;
                    FontIcon.Visibility = Visibility.Visible;
                    if (AgentIcon.Color.HasValue) FontIcon.Foreground = new SolidColorBrush(AgentIcon.Color.Value);
                    break;
            }
        }

        private void IconSizeChanged(DependencyObject _, DependencyProperty __)
        {
            Height = Size;
            Width = Size;

            ImageIcon.Height = Size;
            ImageIcon.Width = Size;
            FontIcon.FontSize = Size;
        }
    }
}
