using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.ViewModels.AI;

namespace PowerPad.WinUI.Components.Controls
{
    public sealed partial class ChatControlParameters : UserControl
    {
        public AIParametersViewModel Parameters
        {
            get => (AIParametersViewModel)GetValue(ParametersProperty);
            set => SetValue(ParametersProperty, value);
        }

        public static readonly DependencyProperty ParametersProperty =
            DependencyProperty.Register(nameof(Parameters), typeof(AIParametersViewModel), typeof(ChatControlParameters), new(null));

        public ChatControlParameters()
        {
            this.InitializeComponent();
        }
    }
}