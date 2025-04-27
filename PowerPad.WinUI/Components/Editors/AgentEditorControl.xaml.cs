using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.Dialogs;
using PowerPad.WinUI.ViewModels.Agents;
using System;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PowerPad.WinUI.Components.Editors
{
    public partial class AgentEditorControl : UserControl, IDisposable
    {
        private readonly AgentViewModel _agent;
        private readonly AgentViewModel _originalAgent;

        public AgentEditorControl(AgentViewModel agent)
        {
            this.InitializeComponent();

            _agent = agent.Copy();
            _originalAgent = agent;
        }

        public async Task<bool> ConfirmClose(XamlRoot xamlRoot)
        {
            if (_agent != _originalAgent)
            { 
                var result = await DialogHelper.Confirm
                (
                    xamlRoot,
                    "Guardar cambios",
                    "El agente ha sido modificado.\n¿Desea guardar los cambios?",
                    showCancel: true
                );

                if (result == ContentDialogResult.Primary)
                {
                    SaveButton_Click(null, null);
                }
                else if (result == ContentDialogResult.None)
                {
                    return false;
                }
            }

            return true;
        }

        private void SaveButton_Click(object? _, RoutedEventArgs? __)
        {
            _originalAgent.SetRecord(_agent.GetRecord());
            _originalAgent.Icon = _agent.Icon;
            _originalAgent.Enabled = _agent.Enabled;
        }

        private void CancelButton_Click(object? _, RoutedEventArgs? __)
        {
            _agent.SetRecord(_originalAgent.GetRecord());
            _agent.Icon = _originalAgent.Icon;
            _agent.Enabled = _originalAgent.Enabled;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
