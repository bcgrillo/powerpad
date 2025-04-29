using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.Dialogs;
using PowerPad.WinUI.ViewModels.Agents;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PowerPad.WinUI.Components.Editors
{
    public partial class AgentEditorControl : UserControl, IDisposable
    {
        private readonly AgentViewModel _agent;
        private readonly AgentViewModel _originalAgent;
        private readonly XamlRoot _xamlRoot;

        public AgentEditorControl(AgentViewModel agent, XamlRoot xamlRoot)
        {
            this.InitializeComponent();

            _agent = agent.Copy();
            _originalAgent = agent;
            _xamlRoot = xamlRoot;

            _agent.PropertyChanged += Agent_PropertyChanged;
        }

        public async Task<bool> ConfirmClose()
        {
            if (_agent != _originalAgent)
            { 
                var result = await DialogHelper.Confirm
                (
                    _xamlRoot,
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

        private void Agent_PropertyChanged(object? _, PropertyChangedEventArgs __)
        {
            SaveButton.IsEnabled = true;
            CancelButton.IsEnabled = true;
        }

        private void SaveButton_Click(object? _, RoutedEventArgs? __)
        {
            _originalAgent.SetRecord(_agent.GetRecord());
            _originalAgent.Icon = _agent.Icon;
            _originalAgent.ShowInNotes = _agent.ShowInNotes;
            _originalAgent.ShowInChats = _agent.ShowInChats;

            SaveButton.IsEnabled = false;
            CancelButton.IsEnabled = false;
        }

        private async void CancelButton_Click(object? _, RoutedEventArgs? __)
        {
            var result = await DialogHelper.Confirm
            (
                _xamlRoot,
                "Deshacer cambios",
                "Perderá los cambios no guardados. ¿Está seguro?",
                showCancel: true
            );

            if (result == ContentDialogResult.Primary)
            {
                _agent.SetRecord(_originalAgent.GetRecord());
                _agent.Icon = _originalAgent.Icon;
                _agent.ShowInNotes = _originalAgent.ShowInNotes;
                _agent.ShowInChats = _originalAgent.ShowInChats;

                SaveButton.IsEnabled = false;
                CancelButton.IsEnabled = false;
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
