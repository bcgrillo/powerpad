using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.ViewModels.AI;

namespace PowerPad.WinUI.Components.Controls
{
    /// <summary>
    /// Represents a user control for configuring AI parameters in the chat interface.
    /// </summary>
    public partial class ChatControlParameters : UserControl
    {
        /// <summary>
        /// Gets or sets the AI parameters used to configure the behavior of the chat system.
        /// </summary>
        public AIParametersViewModel Parameters
        {
            get => (AIParametersViewModel)GetValue(ParametersProperty);
            set => SetValue(ParametersProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="Parameters"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ParametersProperty =
            DependencyProperty.Register(nameof(Parameters), typeof(AIParametersViewModel), typeof(ChatControlParameters), new(null));

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatControlParameters"/> class.
        /// </summary>
        public ChatControlParameters()
        {
            this.InitializeComponent();
        }
    }
}