using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Threading.Tasks;
using System;
using Windows.System;
using Windows.UI.Core;
using System.Threading;
using PowerPad.Core.Services.AI;
using PowerPad.WinUI.ViewModels.Settings;
using PowerPad.WinUI.ViewModels.Agents;
using System.Text;

namespace PowerPad.WinUI.Components.Controls
{
    public partial class NoteAgentControl : UserControl, IDisposable
    {
        private readonly IChatService _chatService;
        private readonly SettingsViewModel _settings;
        private CancellationTokenSource _cts;

        public event EventHandler<RoutedEventArgs>? SendButtonClicked;

        private AgentViewModel? _selectedAgent;

        public NoteAgentControl()
        {
            this.InitializeComponent();

            _chatService = App.Get<IChatService>();
            _settings = App.Get<SettingsViewModel>();
            _cts = new();

            AgentSelector.SelectedAgentChanged += SelectedAgent_Changed;
            AgentSelector.Initialize(null, selectFirstAgent: true);
        }

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

        private void SelectedAgent_Changed(object? _, EventArgs __)
        {
            if (_selectedAgent != AgentSelector.SelectedAgent)
            {
                _selectedAgent = AgentSelector.SelectedAgent;

                UpdateVisibility();
                UpdateParameterInputBox();
                UpdateParameterInputBox();
            }
        }

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

        public void SetFocus()
        {
            if (_selectedAgent?.HasPromptParameter == true) PromptParameterInputBox.Focus(FocusState.Keyboard);
        }

        private void PromptParameterInputBox_TextChanged(object _, TextChangedEventArgs __)
        {
            SendButton.IsEnabled = !string.IsNullOrWhiteSpace(PromptParameterInputBox.Text);
            if (string.IsNullOrEmpty(PromptParameterInputBox.Text)) PromptParameterInputBox.AcceptsReturn = false;
        }

        private void SendBtn_Click(object _, RoutedEventArgs e)
        {
            SendButtonClicked?.Invoke(this, e);
        }

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

                if (_selectedAgent!.HasPromptParameter == true)
                {
                    PromptParameterInputBox.Focus(FocusState.Keyboard);
                }
                else
                {
                    SendButton.IsEnabled = true;
                }
            });
        }

        private void StopBtn_Click(object _, RoutedEventArgs __)
        {
            _cts.Cancel();
            FinalizeAgentAction();
        }

        private void PromptParameterInputBox_KeyDown(object _, KeyRoutedEventArgs e)
        {
            if (!PromptParameterInputBox.IsReadOnly)
            {
                if (e.Key == VirtualKey.Enter)
                {
                    if (!InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift)
                        .HasFlag(CoreVirtualKeyStates.Down) &&
                        !string.IsNullOrWhiteSpace(PromptParameterInputBox.Text))
                    {
                        SendButtonClicked?.Invoke(this, e);
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
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}