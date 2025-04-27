using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using PowerPad.WinUI.Helpers;
using PowerPad.WinUI.ViewModels.Agents;
using System;

namespace PowerPad.WinUI.Components.Controls
{
    public sealed partial class AgentIconControl : UserControl
    {
        private static readonly AgentIcon DEFAULT_AGENT_ICON = new("\uE99A", AgentIconType.FontIconGlyph);

        public AgentIcon? AgentIcon
        {
            get => (AgentIcon)GetValue(AgentIconProperty);
            set => SetValue(AgentIconProperty, value ?? DEFAULT_AGENT_ICON);
        }

        public static readonly DependencyProperty AgentIconProperty =
            DependencyProperty.Register(nameof(AgentIconProperty), typeof(AgentIcon), typeof(AgentIconControl), new(DEFAULT_AGENT_ICON));

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
            TextBlock.Visibility = Visibility.Collapsed;
            FontIcon.Visibility = Visibility.Collapsed;

            switch (AgentIcon!.Value.Type)
            {
                case AgentIconType.Base64Image:
                    ImageIcon.Source = Base64ImageHelper.LoadImageFromBase64(AgentIcon.Value.Source);
                    ImageIcon.Visibility = Visibility.Visible;
                    break;
                case AgentIconType.CharacterOrEmoji:
                    TextBlock.Text = AgentIcon.Value.Source;
                    TextBlock.Visibility = Visibility.Visible;
                    break;
                case AgentIconType.FontIconGlyph:
                    FontIcon.Glyph = AgentIcon.Value.Source;
                    FontIcon.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void IconSizeChanged(DependencyObject _, DependencyProperty __)
        {
            Height = Size;
            Width = Size;

            ImageIcon.Height = Size;
            ImageIcon.Width = Size;
            TextBlock.FontSize = Size;
            FontIcon.FontSize = Size;
        }
    }
}
