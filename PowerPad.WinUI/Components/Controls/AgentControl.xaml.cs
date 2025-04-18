using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Threading.Tasks;
using System;
using Windows.System;
using Windows.UI.Core;
using System.Threading;
using System.Linq;
using PowerPad.Core.Services.AI;
using PowerPad.WinUI.ViewModels.Settings;
using PowerPad.WinUI.ViewModels.Agents;
using System.Text;
using System.Collections.Specialized;

namespace PowerPad.WinUI.Components.Controls
{
    public partial class AgentControl : UserControl
    {
        private readonly IChatService _chatService;
        private readonly AgentsCollectionViewModel _agentsCollection;
        private readonly SettingsViewModel _settings;
        private readonly CancellationTokenSource _cts;

        public event EventHandler<RoutedEventArgs>? SendButtonClicked;

        private AgentViewModel? _selectedAgent;

        public AgentControl()
        {
            this.InitializeComponent();

            _chatService = App.Get<IChatService>();
            _agentsCollection = App.Get<AgentsCollectionViewModel>();
            _settings = App.Get<SettingsViewModel>();
            _cts = new();

            _selectedAgent = _agentsCollection.Agents.FirstOrDefault(a => a.Enabled);
            UpdateVisibility();

            if(AgentPanel.Visibility == Visibility.Visible)
            {
                UpdateAgentsMenu();
                UpdateAgentButtonContent();
                UpdateParameterInputBox();

                ((RadioMenuFlyoutItem)AgentFlyoutMenu.Items.First()).IsChecked = true;
            }

            _agentsCollection.Agents.CollectionChanged += Agents_CollectionChanged;

            _settings.Models.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_settings.Models.DefaultModel)) UpdateVisibility();
            };
        }

        private void UpdateVisibility()
        {
            if (!_settings.IsAIAvailable)
            {
                AgentPanel.Visibility = Visibility.Collapsed;
                InfoBar.IsOpen = true;
                InfoBar.Content = "No hay modelos disponibles, revise la configuración.";
            }
            else if (_selectedAgent is null)
            {
                AgentPanel.Visibility = Visibility.Collapsed;
                InfoBar.IsOpen = true;
                InfoBar.Content = "No hay agentes disponibles, revise la configuración.";
            }
            else if (_selectedAgent is not null && _settings.Models.DefaultModel is not null)
            {
                AgentPanel.Visibility = Visibility.Visible;
                InfoBar.IsOpen = false;
            }
        }

        private void UpdateAgentsMenu()
        {
            AgentFlyoutMenu.Items.Clear();

            var enabledAgents = _agentsCollection.Agents.Where(a => a.Enabled);

            foreach (var agent in enabledAgents)
            {
                var menuItem = new RadioMenuFlyoutItem
                {
                    Text = agent.Name,
                    Tag = agent,
                    Icon = agent.IconElement
                };

                AgentFlyoutMenu.Items.Add(menuItem);

                menuItem.Click += SetModelItem_Click;
            }
        }

        private void SetModelItem_Click(object sender, RoutedEventArgs _)
        {
            _selectedAgent = (AgentViewModel?)((RadioMenuFlyoutItem)sender).Tag;

            ((RadioMenuFlyoutItem)sender).IsChecked = true;

            UpdateAgentButtonContent();
            UpdateParameterInputBox();
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

        private void UpdateAgentButtonContent()
        {
            if (_selectedAgent is not null)
            {
                ModelIcon.Content = _selectedAgent.IconElement;
                ModelName.Text = _selectedAgent.Name;
            }
            else
            {
                ModelIcon.Content = null;
                ModelName.Text = "Unavailable";
            }
        }

        private void Agents_CollectionChanged(object? _, NotifyCollectionChangedEventArgs __)
        {
            if (_selectedAgent is null)
                _selectedAgent ??= _agentsCollection.Agents.FirstOrDefault(a => a.Enabled);
            else
                _selectedAgent = _agentsCollection.Agents.FirstOrDefault(a => a.Enabled && a.Name == _selectedAgent.Name);

            UpdateVisibility();

            if (AgentPanel.Visibility == Visibility.Visible)
            {
                UpdateAgentsMenu();
                UpdateAgentButtonContent();
                UpdateParameterInputBox();

                var menuItem = (RadioMenuFlyoutItem)AgentFlyoutMenu.Items.First(i => i.Tag as AgentViewModel == _selectedAgent);

                DispatcherQueue.TryEnqueue(async () =>
                {
                    await Task.Delay(100);
                    menuItem.IsChecked = true;
                });
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

        public async Task StartAgentAction(string input, StringBuilder output)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                SendButton.Visibility = Visibility.Collapsed;
                StopButton.Visibility = Visibility.Visible;
                PromptParameterInputBox.IsReadOnly = true;
                AgentButton.IsEnabled = false;
            });

            _cts.TryReset();

            await _chatService.GetAgentResponse(input, output, _selectedAgent!.GetRecord(), PromptParameterInputBox.Text, _settings.General.AgentPrompt, _cts.Token);

            if (!_cts.IsCancellationRequested) FinalizeAgentAction();
        }

        private void FinalizeAgentAction()
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                StopButton.Visibility = Visibility.Collapsed;
                SendButton.Visibility = Visibility.Visible;
                PromptParameterInputBox.Text = string.Empty;
                PromptParameterInputBox.IsReadOnly = false;
                AgentButton.IsEnabled = true;

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
    }
}