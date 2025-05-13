using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using PowerPad.Core.Services.AI;
using PowerPad.WinUI.ViewModels.Agents;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;

namespace PowerPad.WinUI.Components.Controls
{
    /// <summary>
    /// Represents a control for interacting with AI agents in the NoteAgent system.
    /// </summary>
    public partial class NoteAgentControl : UserControl, IDisposable
    {
        private readonly IChatService _chatService;
        private readonly SettingsViewModel _settings;
        private CancellationTokenSource? _cts;

        /// <summary>
        /// Event triggered when the send button is clicked.
        /// </summary>
        public event EventHandler<RoutedEventArgs>? SendButtonClicked;

        private AgentViewModel? _selectedAgent;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteAgentControl"/> class.
        /// </summary>
        public NoteAgentControl()
        {
            this.InitializeComponent();

            _chatService = App.Get<IChatService>();
            _settings = App.Get<SettingsViewModel>();

            AgentSelector.SelectedAgentChanged += SelectedAgent_Changed;
            AgentSelector.Initialize(null, selectFirstAgent: true);
        }

        /// <summary>
        /// Sets focus to the appropriate input element based on the selected agent's configuration.
        /// </summary>
        public void SetFocus()
        {
            if (_selectedAgent?.HasPromptParameter == true) PromptParameterInputBox.Focus(FocusState.Keyboard);
        }

        /// <summary>
        /// Starts an action for the selected agent asynchronously.
        /// </summary>
        /// <param name="input">The input text for the agent.</param>
        /// <param name="output">The output StringBuilder to store the agent's response.</param>
        /// <param name="exceptionAction">The action to handle exceptions.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task StartAgentAction(string input, StringBuilder output, Action<Exception> exceptionAction)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                SendButton.Visibility = Visibility.Collapsed;
                AgentProgress.Visibility = Visibility.Visible;
                StopButton.Visibility = Visibility.Visible;
                PromptParameterInputBox.IsReadOnly = true;
                AgentSelector.IsEnabled = false;
            });

            _cts?.Dispose();
            _cts = new();

            try
            {
                await _chatService.GetAgentSingleResponse(input, output, _selectedAgent!.GetRecord(), PromptParameterInputBox.Text, _settings.General.AgentPrompt, _cts.Token);
            }
            catch (Exception ex)
            {
                exceptionAction(ex);
            }

            if (!_cts.IsCancellationRequested) FinalizeAgentAction();
        }

        /// <summary>
        /// Updates the visibility of UI elements based on the selected agent.
        /// </summary>
        private void UpdateVisibility()
        {
            if (_selectedAgent is null)
            {
                AgentPanel.Visibility = Visibility.Collapsed;
                InfoBar.IsOpen = true;
                InfoBar.Content = "No hay agentes disponibles, revise la configuración.";
            }
            else if (_selectedAgent is not null)
            {
                AgentPanel.Visibility = Visibility.Visible;
                InfoBar.IsOpen = false;
            }
        }

        /// <summary>
        /// Handles the event when the selected agent changes.
        /// </summary>
        private void SelectedAgent_Changed(object? _, EventArgs __)
        {
            if (_selectedAgent != AgentSelector.SelectedAgent)
            {
                _selectedAgent = AgentSelector.SelectedAgent;

                UpdateVisibility();
                UpdateParameterInputBox();
            }
        }

        /// <summary>
        /// Updates the parameter input box based on the selected agent's configuration.
        /// </summary>
        private void UpdateParameterInputBox()
        {
            if (_selectedAgent!.HasPromptParameter)
            {
                PromptParameterInputBox.PlaceholderText = _selectedAgent.PromptParameterDescription;
                PromptParameterInputBox.Visibility = Visibility.Visible;
                SendButton.IsEnabled = false;
            }
            else
            {
                PromptParameterInputBox.Visibility = Visibility.Collapsed;
                SendButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Handles the text changed event for the parameter input box.
        /// </summary>
        private void PromptParameterInputBox_TextChanged(object _, TextChangedEventArgs __)
        {
            SendButton.IsEnabled = !string.IsNullOrWhiteSpace(PromptParameterInputBox.Text);
            if (string.IsNullOrEmpty(PromptParameterInputBox.Text)) PromptParameterInputBox.AcceptsReturn = false;
        }

        /// <summary>
        /// Handles the click event for the send button.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="eventArgs">The event arguments for the click event.</param>
        private void SendBtn_Click(object _, RoutedEventArgs eventArgs)
        {
            SendButtonClicked?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Finalizes the agent action by resetting the UI state.
        /// </summary>
        private void FinalizeAgentAction()
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                AgentProgress.Visibility = Visibility.Collapsed;
                StopButton.Visibility = Visibility.Collapsed;
                SendButton.Visibility = Visibility.Visible;
                PromptParameterInputBox.Text = string.Empty;
                PromptParameterInputBox.IsReadOnly = false;
                AgentSelector.IsEnabled = true;

                if (_selectedAgent!.HasPromptParameter)
                {
                    PromptParameterInputBox.Focus(FocusState.Keyboard);
                }
                else
                {
                    SendButton.IsEnabled = true;
                }
            });
        }

        /// <summary>
        /// Handles the click event for the stop button.
        /// </summary>
        private void StopBtn_Click(object _, RoutedEventArgs __)
        {
            _cts?.Cancel();
            FinalizeAgentAction();
        }

        /// <summary>
        /// Handles the key down event for the parameter input box.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="eventArgs">The key event arguments.</param>
        private void PromptParameterInputBox_KeyDown(object _, KeyRoutedEventArgs eventArgs)
        {
            if (!PromptParameterInputBox.IsReadOnly && eventArgs.Key == VirtualKey.Enter)
            {
                if (!InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift)
                        .HasFlag(CoreVirtualKeyStates.Down) &&
                        !string.IsNullOrWhiteSpace(PromptParameterInputBox.Text))
                {
                    SendButtonClicked?.Invoke(this, eventArgs);
                }
                else
                {
                    PromptParameterInputBox.AcceptsReturn = true;
                    var cursorPosition = PromptParameterInputBox.SelectionStart;
                    PromptParameterInputBox.Text = PromptParameterInputBox.Text.Insert(cursorPosition, Environment.NewLine);
                    PromptParameterInputBox.SelectionStart = cursorPosition + Environment.NewLine.Length;
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes resources used by the control.
        /// </summary>
        /// <param name="disposing">Indicates whether the method is called from Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                AgentSelector.SelectedAgentChanged += SelectedAgent_Changed;
                _cts?.Dispose();
            }
        }
    }
}