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
    /// <summary>
    /// Represents a control for editing an AI agent's properties and settings.
    /// </summary>
    public partial class AgentEditorControl : UserControl, IDisposable
    {
        private readonly SettingsViewModel _settings;
        private readonly AgentsCollectionViewModel _agentsCollection;
        private readonly AgentViewModel _agent;
        private readonly AgentViewModel _originalAgent;
        private readonly XamlRoot _xamlRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentEditorControl"/> class.
        /// </summary>
        /// <param name="agent">The agent to be edited.</param>
        /// <param name="xamlRoot">The XAML root for dialog placement.</param>
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

        /// <summary>
        /// Confirms whether the user wants to close the editor, prompting to save changes if necessary.
        /// </summary>
        /// <returns>A task that resolves to <c>true</c> if the editor can be closed; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Handles the click event for selecting an image for the agent.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="__">The event arguments (not used).</param>
        private async void SelectImageButton_Click(object? _, RoutedEventArgs? __)
        {
            var base64Image = await Base64ImageHelper.PickImageToBase64(_xamlRoot);

            if (!string.IsNullOrEmpty(base64Image))
            {
                _agent.Icon = new AgentIcon(base64Image, AgentIconType.Base64Image);
            }
        }

        /// <summary>
        /// Handles the click event for generating a random icon for the agent.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="__">The event arguments (not used).</param>
        private void RandomIconButton_Click(object? _, RoutedEventArgs? __)
        {
            _agent.Icon = _agentsCollection.GenerateIcon();
        }

        /// <summary>
        /// Handles property changes in the agent and enables the Save and Cancel buttons.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="__">The event arguments (not used).</param>
        private void Agent_PropertyChanged(object? _, PropertyChangedEventArgs __)
        {
            SaveButton.IsEnabled = true;
            CancelButton.IsEnabled = true;
        }

        /// <summary>
        /// Saves the changes made to the agent and disables the Save and Cancel buttons.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="__">The event arguments (not used).</param>
        private void SaveButton_Click(object? _, RoutedEventArgs? __)
        {
            _originalAgent.SetRecord(_agent.GetRecord());
            _originalAgent.Icon = _agent.Icon;
            _originalAgent.ShowInNotes = _agent.ShowInNotes;
            _originalAgent.ShowInChats = _agent.ShowInChats;

            if (!AIParametersSwitch.IsOn)
            {
                _originalAgent.Temperature = null;
                _originalAgent.TopP = null;
                _originalAgent.MaxOutputTokens = null;
            }

            SaveButton.IsEnabled = false;
            CancelButton.IsEnabled = false;
        }

        /// <summary>
        /// Cancels the changes made to the agent, restoring the original values.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="__">The event arguments (not used).</param>
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

                PromptParameterExpander.IsExpanded = _agent.HasPromptParameter;
                PromptParameterControls.IsEnabled = _agent.HasPromptParameter;

                PromptParameterExpander.IsExpanded = _agent.HasAIParameters;
                PromptParameterControls.IsEnabled = _agent.HasAIParameters;

                SaveButton.IsEnabled = false;
                CancelButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// Handles changes in the selected AI model.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="__">The event arguments (not used).</param>
        private void ModelSelector_SelectedModelChanged(object _, EventArgs __)
        {
            _agent.AIModel = ModelSelector.SelectedModel?.GetRecord();
        }

        /// <summary>
        /// Toggles the visibility and state of the prompt parameter controls.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="__">The event arguments (not used).</param>
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
                PromptParameterExpander.IsExpanded = false;
                PromptParameterControls.IsEnabled = false;

                _agent.PromptParameterName = null;
                _agent.PromptParameterDescription = null;
            }

            DispatcherQueue.TryEnqueue(async () =>
            {
                await Task.Delay(PromptParameterSwitch.IsOn ? 100 : 200);
                UpdateScrollViewerMargin();
            });

            var hasChanges = PromptParameterSwitch.IsOn != _originalAgent.HasPromptParameter;
            SaveButton.IsEnabled = hasChanges;
            CancelButton.IsEnabled = hasChanges;
        }

        /// <summary>
        /// Toggles the visibility and state of the AI parameters controls.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="__">The event arguments (not used).</param>
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
                AIParametersExpander.IsExpanded = false;
                AIParametersControls.IsEnabled = false;
            }

            DispatcherQueue.TryEnqueue(async () =>
            {
                await Task.Delay(AIParametersSwitch.IsOn ? 100 : 200);

                UpdateScrollViewerMargin();
            });

            var hasChanges = AIParametersSwitch.IsOn != _originalAgent.HasAIParameters;
            SaveButton.IsEnabled = hasChanges;
            CancelButton.IsEnabled = hasChanges;
        }

        /// <summary>
        /// Handles text changes in the agent name text box.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="__">The event arguments (not used).</param>
        private void AgentNameTextBox_TextChanging(TextBox _, TextBoxTextChangingEventArgs __)
        {
            if (AgentNameTextBox.Text != _originalAgent.Name)
            {
                SaveButton.IsEnabled = true;
                CancelButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Handles text changes in the agent prompt text box.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="__">The event arguments (not used).</param>
        private void AgentPromptTextBox_TextChanging(TextBox _, TextBoxTextChangingEventArgs __)
        {
            if (AgentPromptTextBox.Text != _originalAgent.Prompt)
            {
                SaveButton.IsEnabled = true;
                CancelButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Handles size changes in the scroll viewer and updates the margin.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="__">The event arguments (not used).</param>
        private void ScrollViewer_SizeChanged(object _, SizeChangedEventArgs __) => UpdateScrollViewerMargin();

        /// <summary>
        /// Updates the margin of the scroll viewer based on its visibility and width.
        /// </summary>
        private void UpdateScrollViewerMargin()
        {
            var changeMargin = ScrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible
                && ScrollViewer.ActualWidth < 1024;

            AgentForm.Margin = AgentForm.Margin with { Left = 0, Right = changeMargin ? 24 : 0 };
        }

        /// <summary>
        /// Disposes of resources used by the control.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of resources used by the control.
        /// </summary>
        /// <param name="_">A boolean indicating whether to dispose managed resources (not used).</param>
        protected virtual void Dispose(bool _)
        {
            // Nothing to dispose in this case
        }
    }
}