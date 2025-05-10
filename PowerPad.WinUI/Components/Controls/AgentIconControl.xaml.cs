using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using PowerPad.WinUI.Helpers;
using PowerPad.WinUI.ViewModels.Agents;

namespace PowerPad.WinUI.Components.Controls
{
    /// <summary>
    /// A custom control for displaying an agent icon, which can be either a Base64 image or a font glyph.
    /// </summary>
    public partial class AgentIconControl : UserControl
    {
        /// <summary>
        /// Gets or sets the AgentIcon object that defines the type and source of the icon.
        /// </summary>
        public AgentIcon AgentIcon
        {
            get => (AgentIcon)GetValue(AgentIconProperty);
            set => SetValue(AgentIconProperty, value);
        }

        /// <summary>
        /// DependencyProperty for the AgentIcon property.
        /// </summary>
        public static readonly DependencyProperty AgentIconProperty =
            DependencyProperty.Register(nameof(AgentIconProperty), typeof(AgentIcon), typeof(AgentIconControl), new(default));

        /// <summary>
        /// Gets or sets the size of the icon.
        /// </summary>
        public double Size
        {
            get => (double)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        /// <summary>
        /// DependencyProperty for the Size property.
        /// </summary>
        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register(nameof(SizeProperty), typeof(double), typeof(AgentIconControl), new(20));

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentIconControl"/> class.
        /// </summary>
        public AgentIconControl()
        {
            this.InitializeComponent();

            RegisterPropertyChangedCallback(AgentIconProperty, AgentIconChanged);
            RegisterPropertyChangedCallback(SizeProperty, IconSizeChanged);
        }

        /// <summary>
        /// Callback invoked when the AgentIcon property changes.
        /// Updates the visibility and content of the icon based on its type.
        /// </summary>
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

        /// <summary>
        /// Callback invoked when the Size property changes.
        /// Updates the dimensions of the control and its child elements.
        /// </summary>
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