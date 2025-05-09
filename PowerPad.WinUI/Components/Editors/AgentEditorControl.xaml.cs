using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.Dialogs;
using PowerPad.WinUI.Helpers;
using PowerPad.WinUI.ViewModels.Agents;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Components.Editors
{
    public partial class AgentEditorControl : UserControl, IDisposable
    {
        private readonly SettingsViewModel _settings;
        private readonly AgentsCollectionViewModel _agentsCollection;
        private readonly AgentViewModel _agent;
        private readonly AgentViewModel _originalAgent;
        private readonly XamlRoot _xamlRoot;

        public AgentEditorControl(AgentViewModel agent, XamlRoot xamlRoot)
        {
            this.InitializeComponent();

            _settings = App.Get<SettingsViewModel>();
            _agentsCollection = App.Get<AgentsCollectionViewModel>();
            _agent = agent.Copy();
            _originalAgent = agent;
            _xamlRoot = xamlRoot;

            _agent.PropertyChanged += Agent_PropertyChanged;

            ModelSelector.Initialize(_agent.AIModel is not null ? new(_agent.AIModel) : null);

            AgentNameTextBox.TextChanging += AgentNameTextBox_TextChanging;
            AgentPromptTextBox.TextChanging += AgentPromptTextBox_TextChanging;
        }

        public async Task<bool> ConfirmClose()
        {
            if (SaveButton.IsEnabled)
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

        private async void SelectImageButton_Click(object? _, RoutedEventArgs? __)
        {
            var base64Image = await Base64ImageHelper.PickImageToBase64(_xamlRoot);

            if (!string.IsNullOrEmpty(base64Image))
            {
                _agent.Icon = new AgentIcon(base64Image, AgentIconType.Base64Image);
            }
        }

        private void RandonIconButton_Click(object? _, RoutedEventArgs? __)
        {
            _agent.Icon = _agentsCollection.GenerateIcon();
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

        private void ModelSelector_SelectedModelChanged(object _, EventArgs __)
        {
            _agent.AIModel = ModelSelector.SelectedModel?.GetRecord();
        }

        private void PromptParameterSwitch_Toggled(object _, RoutedEventArgs __)
        {
            if (PromptParameterSwitch.IsOn)
            {
                _agent.PromptParameterName = _originalAgent.PromptParameterName ?? string.Empty;
                _agent.PromptParameterDescription = _originalAgent.PromptParameterDescription ?? string.Empty;

                PromptParameterExpander.IsExpanded = true;
                PromptParameterControls.IsEnabled = true;
            }
            else
            {
                _agent.PromptParameterName = null;
                _agent.PromptParameterDescription = null;

                PromptParameterExpander.IsExpanded = false;
                PromptParameterControls.IsEnabled = false;
            }

            DispatcherQueue.TryEnqueue(async () =>
            {
                await Task.Delay(PromptParameterSwitch.IsOn ? 100 : 200);
                UpdateScrollViewerMargin();
            });
        }

        private void AIParametersSwitch_Toggled(object _, RoutedEventArgs __)
        {
            if (AIParametersSwitch.IsOn)
            {
                _agent.Temperature = _originalAgent.Temperature ?? _settings.Models.DefaultParameters.Temperature;
                _agent.TopP = _originalAgent.TopP ?? _settings.Models.DefaultParameters.TopP;
                _agent.MaxOutputTokens = _originalAgent.MaxOutputTokens ?? _settings.Models.DefaultParameters.MaxOutputTokens;

                AIParametersExpander.IsExpanded = true;
                AIParametersControls.IsEnabled = true;
            }
            else
            {
                _agent.Temperature = null;
                _agent.TopP = null;
                _agent.MaxOutputTokens = null;

                AIParametersExpander.IsExpanded = false;
                AIParametersControls.IsEnabled = false;
            }

            DispatcherQueue.TryEnqueue(async () =>
            {
                await Task.Delay(AIParametersSwitch.IsOn ? 100 : 200);
                UpdateScrollViewerMargin();
            });
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private void AgentNameTextBox_TextChanging(TextBox _, TextBoxTextChangingEventArgs __)
        {
            if (AgentNameTextBox.Text != _originalAgent.Name)
            {
                SaveButton.IsEnabled = true;
                CancelButton.IsEnabled = true;
            }
        }

        private void AgentPromptTextBox_TextChanging(TextBox _, TextBoxTextChangingEventArgs __)
        {
            if (AgentPromptTextBox.Text != _originalAgent.Prompt)
            {
                SaveButton.IsEnabled = true;
                CancelButton.IsEnabled = true;
            }
        }

        private void ScrollViewer_SizeChanged(object _, SizeChangedEventArgs __) => UpdateScrollViewerMargin();

        private void UpdateScrollViewerMargin()
        {
            var changeMargin = ScrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible
                && ScrollViewer.ActualWidth < 1024;

            AgentForm.Margin = AgentForm.Margin with { Left = 0, Right = changeMargin ? 24 : 0 };
        }
    }
}