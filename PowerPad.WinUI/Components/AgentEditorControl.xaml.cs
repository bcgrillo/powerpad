using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.ViewModels.Agents;

namespace PowerPad.WinUI.Components
{
    public sealed partial class AgentEditorControl : UserControl
    {
        private readonly AgentViewModel _agent;

        public AgentEditorControl(AgentViewModel agent)
        {
            this.InitializeComponent();

            _agent = agent;

        }
    }
}
