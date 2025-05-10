using Microsoft.Extensions.AI;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using PowerPad.Core.Services.AI;
using PowerPad.WinUI.ViewModels.Agents;
using PowerPad.WinUI.ViewModels.AI;
using PowerPad.WinUI.ViewModels.Chat;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;

namespace PowerPad.WinUI.Components.Controls
{
    /// <summary>
    /// Represents a control for managing chat interactions, including AI model and agent selection, parameter configuration, and message streaming.
    /// </summary>
    public partial class ChatControl : UserControl, IDisposable
    {
        private const double LOADING_ANIMATION_INTERVAL = 200;
        private static readonly string[] THINK_START_TAG = ["<think>", "<thought>"];
        private static readonly string[] THINK_END_TAG = ["</think>", "</thought>"];

        private readonly IChatService _chatService;
        private readonly SettingsViewModel _settings;
        private readonly DispatcherTimer _loadingAnimationTimer;
        private readonly AIParametersViewModel _parameters;

        private CancellationTokenSource? _cts;
        private int _loadingStep = 0;
        private Action? _finalizeChatAction;
        private ICollection<MessageViewModel>? _messageList;
        private MessageViewModel? _lastUserMessage;
        private MessageViewModel? _lastAssistantMessage;
        private AIModelViewModel? _selectedModel;
        private bool _sendParameters;
        private AgentViewModel? _selectedAgent;
        private bool _useAgents;

        public event EventHandler<RoutedEventArgs>? SendButtonClicked;
        public event EventHandler<ChatOptionsChangedEventArgs>? ChatOptionsChanged;
        public event EventHandler<bool>? ParametersVisibilityChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatControl"/> class.
        /// </summary>
        public ChatControl()
        {
            this.InitializeComponent();

            _chatService = App.Get<IChatService>();
            _settings = App.Get<SettingsViewModel>();

            _loadingAnimationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(LOADING_ANIMATION_INTERVAL)
            };
            _loadingAnimationTimer.Tick += LoadingAnimationTimer_Tick;

            _parameters = _settings.Models.DefaultParameters.Copy();

            IsEnabledChanged += OnEnabledChanged;
        }

        /// <summary>
        /// Initializes the parameters for the chat control, including model, parameters, and agent.
        /// </summary>
        /// <param name="model">The AI model to use for the chat.</param>
        /// <param name="parameters">The parameters for the chat. Can be null.</param>
        /// <param name="agentId">The ID of the agent to use. Can be null.</param>
        public void InitializeParameters(AIModelViewModel? model, AIParametersViewModel? parameters, Guid? agentId)
        {
            _selectedModel = model;
            ModelSelector.SelectedModelChanged += SelectedModel_Changed;
            ModelSelector.Initialize(model);

            if (parameters is not null)
            {
                _parameters.SetRecord(parameters.GetRecord());

                _sendParameters = true;
                EnableParametersSwitch.IsOn = true;
                ToggleParameterVisibility();
            }
            _parameters.PropertyChanged += Parameters_PropertyChanged;

            _selectedAgent = agentId.HasValue
                ? App.Get<AgentsCollectionViewModel>().GetAgent(agentId.Value)
                : null;
            AgentSelector.SelectedAgentChanged += SelectedAgent_Changed;
            AgentSelector.Initialize(_selectedAgent);

            if (agentId.HasValue)
            {
                _useAgents = true;
                UpdateChatButtonsLayout();
            }
        }

        /// <summary>
        /// Sets focus to the chat input box.
        /// </summary>
        public void SetFocus()
        {
            ChatInputBox.Focus(FocusState.Keyboard);
        }

        /// <summary>
        /// Starts streaming chat messages and updates the UI accordingly.
        /// </summary>
        /// <param name="messageList">The list of messages to display in the chat.</param>
        /// <param name="endAction">The action to execute when the chat ends. Can be null.</param>
        public void StartStreamingChat(ICollection<MessageViewModel> messageList, Action? endAction)
        {
            _messageList = messageList;
            _finalizeChatAction = endAction;
            _loadingAnimationTimer.Start();

            _lastUserMessage = new(ChatInputBox.Text.Trim().Replace("\r", "  \r"), DateTime.Now, ChatRole.User);
            messageList.Add(_lastUserMessage);

            _ = Task.Run(async () =>
            {
                var history = messageList.Select(m => new ChatMessage(m.Role, m.Content)).ToList();
                _lastAssistantMessage = new MessageViewModel(null, DateTime.Now, ChatRole.Assistant) { Loading = true };

                DispatcherQueue.TryEnqueue(() =>
                {
                    messageList.Add(_lastAssistantMessage);
                    SendButton.Visibility = Visibility.Collapsed;
                    StopButton.Visibility = Visibility.Visible;
                    ChatInputBox.IsReadOnly = true;
                    ModelSelector.IsEnabled = false;
                    AgentSelector.IsEnabled = false;
                    ParametersButton.IsEnabled = false;
                });

                _cts = new();

                var parameters = _sendParameters ? _parameters
                    : (_settings.Models.SendDefaultParameters ? _settings.Models.DefaultParameters : null);

                var messageBuffer = new StringBuilder();
                try
                {
                    var responseUpdates = _useAgents
                        ? _chatService.GetAgentResponse(history, _selectedAgent!.GetRecord(), _cts.Token)
                        : _chatService.GetChatResponse(history, _selectedModel?.GetRecord(), _sendParameters ? parameters?.GetRecord() : null, _cts.Token);

                    await foreach (var update in responseUpdates)
                    {
                        messageBuffer.Append(update.Text);
                        var messageBufferString = messageBuffer.ToString();

                        string? reasoning = null;
                        string? content = null;
                        bool loading = true;

                        var thinkStartTag = THINK_START_TAG.FirstOrDefault(tag => messageBufferString.Contains(tag));

                        // Logic to parse and update reasoning and content from the message buffer
                        if (thinkStartTag is not null)
                        {
                            var thinkEndTag = THINK_END_TAG.FirstOrDefault(tag => messageBufferString.Contains(tag));

                            var startIndex = messageBufferString.IndexOf(thinkStartTag) + thinkStartTag.Length;

                            if (thinkEndTag is not null)
                            {
                                var endIndex = messageBufferString.IndexOf(thinkEndTag, startIndex);

                                reasoning = messageBufferString[startIndex..endIndex].Trim().Replace("\n\n", "\n");
                                content = messageBufferString[(endIndex + thinkEndTag.Length)..].Trim();

                                if (loading) loading = false;
                            }
                            else
                            {
                                reasoning = messageBufferString[startIndex..].Trim().Replace("\n\n", "\n");
                            }
                        }
                        else
                        {
                            content = messageBufferString;
                            if (loading) loading = false;
                        }

                        DispatcherQueue.TryEnqueue(() =>
                        {
                            _lastAssistantMessage.Reasoning = reasoning;
                            _lastAssistantMessage.Content = content;
                            _lastAssistantMessage.Loading = loading;
                        });
                    }
                }
                catch (TaskCanceledException)
                {
                    // Cancellation is expected, no action needed
                }
                catch (Exception ex)
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        messageList.Remove(_lastAssistantMessage);

                        _lastAssistantMessage.Reasoning = null;
                        _lastAssistantMessage.Content = null;
                        _lastAssistantMessage.Loading = false;
                        _lastAssistantMessage.ErrorMessage = $"Error: {ex.Message.Trim().ReplaceLineEndings(" ")}";
                        messageList.Add(_lastAssistantMessage);
                    });
                }

                if (!_cts.IsCancellationRequested) FinalizeChat();
            });
        }

        /// <summary>
        /// Handles the event when the control's enabled state changes.
        /// </summary>
        /// <param name="eventArgs">The event arguments containing the new value.</param>
        private void OnEnabledChanged(object? _, DependencyPropertyChangedEventArgs eventArgs)
        {
            ModelSelector.UpdateEnabledLayout((bool)eventArgs.NewValue);
            ParametersIcon.UpdateEnabledLayout((bool)eventArgs.NewValue);
        }

        /// <summary>
        /// Handles the event when the selected model changes.
        /// </summary>
        private void SelectedModel_Changed(object? _, EventArgs __)
        {
            if (_selectedModel != ModelSelector.SelectedModel)
            {
                _selectedModel = ModelSelector.SelectedModel;
                OnChatOptionsChanged();
            }
        }

        /// <summary>
        /// Handles the event when the selected agent changes.
        /// </summary>
        private void SelectedAgent_Changed(object? _, EventArgs __)
        {
            if (_useAgents && _selectedAgent != AgentSelector.SelectedAgent)
            {
                _selectedAgent = AgentSelector.SelectedAgent;

                SendButton.IsEnabled = !string.IsNullOrWhiteSpace(ChatInputBox.Text)
                    && (!_useAgents || _selectedAgent is not null);

                OnChatOptionsChanged();
            }
        }

        /// <summary>
        /// Toggles the use of agents and updates the chat button layout accordingly.
        /// </summary>
        private void AgentToggleButton_Click(object _, RoutedEventArgs __)
        {
            _useAgents = !_useAgents;

            OnChatOptionsChanged();

            UpdateChatButtonsLayout();

            if (_useAgents && _selectedAgent is null) AgentSelector.ShowMenu();
        }

        /// <summary>
        /// Updates the layout of chat buttons based on the current state of agent usage.
        /// </summary>
        private void UpdateChatButtonsLayout()
        {
            if (_useAgents)
            {
                AgentSelector.Visibility = Visibility.Visible;
                ModelSelector.Visibility = Visibility.Collapsed;
                ParametersButton.Visibility = Visibility.Collapsed;
                ((Border)AgentToggleButton.Content).BorderBrush = (Brush)Application.Current.Resources["AccentFillColorDefaultBrush"];
            }
            else
            {
                AgentSelector.Visibility = Visibility.Collapsed;
                ModelSelector.Visibility = Visibility.Visible;
                ModelSelector.UpdateEnabledLayout(true);
                ParametersButton.Visibility = Visibility.Visible;
                ((Border)AgentToggleButton.Content).BorderBrush = new SolidColorBrush(Colors.Transparent);
            }
        }

        /// <summary>
        /// Handles the event when the text in the chat input box changes.
        /// </summary>
        private void ChatInputBox_TextChanged(object _, TextChangedEventArgs __)
        {
            SendButton.IsEnabled = !string.IsNullOrWhiteSpace(ChatInputBox.Text)
                && (!_useAgents || _selectedAgent is not null);

            if (string.IsNullOrEmpty(ChatInputBox.Text)) ChatInputBox.AcceptsReturn = false;
        }

        /// <summary>
        /// Handles the click event of the send button.
        /// </summary>
        /// <param name="eventArgs">The event arguments.</param>
        private void SendBtn_Click(object _, RoutedEventArgs eventArgs)
        {
            SendButtonClicked?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Finalizes the chat session, resetting the UI and clearing resources.
        /// </summary>
        private void FinalizeChat()
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (_lastAssistantMessage is not null
                    && string.IsNullOrWhiteSpace(_lastAssistantMessage.Content)
                    && string.IsNullOrWhiteSpace(_lastAssistantMessage.ErrorMessage))
                {
                    _messageList!.Remove(_lastAssistantMessage);
                    _messageList!.Remove(_lastUserMessage!);
                }
                else
                {
                    ChatInputBox.Text = string.Empty;
                }

                StopButton.Visibility = Visibility.Collapsed;
                SendButton.Visibility = Visibility.Visible;
                ChatInputBox.IsReadOnly = false;
                ModelSelector.IsEnabled = true;
                AgentSelector.IsEnabled = true;
                ParametersButton.IsEnabled = true;
                ChatInputBox.Focus(FocusState.Keyboard);

                _finalizeChatAction?.Invoke();
                _finalizeChatAction = null;
                _messageList = null;
                _lastUserMessage = null;
                _lastAssistantMessage = null;
                _loadingAnimationTimer.Stop();
            });
        }

        /// <summary>
        /// Handles the click event of the stop button, canceling the current chat session.
        /// </summary>
        private void StopBtn_Click(object _, RoutedEventArgs __)
        {
            _cts!.Cancel();
            FinalizeChat();
        }

        /// <summary>
        /// Handles the key down event for the chat input box, enabling multi-line input or sending messages.
        /// </summary>
        /// <param name="eventArgs">The event arguments.</param>
        private void ChatInputBox_KeyDown(object _, KeyRoutedEventArgs eventArgs)
        {
            if (!ChatInputBox.IsReadOnly && eventArgs.Key == VirtualKey.Enter)
            {
                if (!InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift)
                        .HasFlag(CoreVirtualKeyStates.Down) &&
                        !string.IsNullOrWhiteSpace(ChatInputBox.Text))
                {
                    SendButtonClicked?.Invoke(this, eventArgs);
                }
                else
                {
                    ChatInputBox.AcceptsReturn = true;
                    var cursorPosition = ChatInputBox.SelectionStart;
                    ChatInputBox.Text = ChatInputBox.Text.Insert(cursorPosition, Environment.NewLine);
                    ChatInputBox.SelectionStart = cursorPosition + Environment.NewLine.Length;
                }
            }
        }

        /// <summary>
        /// Toggles the visibility of the parameters panel.
        /// </summary>
        /// <param name="_">The sender of the event.</param>
        /// <param name="__">The event arguments.</param>
        private void ParametersButton_Click(object _, RoutedEventArgs __)
        {
            var parameterPanelVisible = ParametersPanel.Visibility == Visibility.Visible;

            if (parameterPanelVisible)
            {
                ParametersPanel.Visibility = Visibility.Collapsed;
                ChatInputBox.IsEnabled = true;
                SendButton.IsEnabled = !string.IsNullOrWhiteSpace(ChatInputBox.Text);
            }
            else
            {
                ParametersPanel.Visibility = Visibility.Visible;
                ChatInputBox.IsEnabled = false;
                SendButton.IsEnabled = false;
            }

            ParametersVisibilityChanged?.Invoke(this, !parameterPanelVisible);
        }

        /// <summary>
        /// Toggles the parameter visibility and updates the chat options.
        /// </summary>
        /// <param name="_">The sender of the event.</param>
        /// <param name="__">The event arguments.</param>
        private void EnableParametersSwitch_Toggled(object _, RoutedEventArgs __)
        {
            ToggleParameterVisibility();
            OnChatOptionsChanged();
        }

        /// <summary>
        /// Toggles the visibility of custom parameters.
        /// </summary>
        private void ToggleParameterVisibility()
        {
            if (EnableParametersSwitch.IsOn)
            {
                _sendParameters = true;
                ControlDefaultParamters.Visibility = Visibility.Collapsed;
                ControlCustomParamters.Visibility = Visibility.Visible;
                ((Border)ParametersButton.Content).BorderBrush = (Brush)Application.Current.Resources["AccentFillColorDefaultBrush"];
            }
            else
            {
                _sendParameters = false;
                ControlDefaultParamters.Visibility = Visibility.Visible;
                ControlCustomParamters.Visibility = Visibility.Collapsed;
                ((Border)ParametersButton.Content).BorderBrush = new SolidColorBrush(Colors.Transparent);
            }
        }

        /// <summary>
        /// Handles the click event of the close parameters button.
        /// </summary>
        /// <param name="o">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void CloseParametersButton_Click(object o, RoutedEventArgs eventArgs)
        {
            ParametersButton.IsChecked = false;
            ParametersButton_Click(o, eventArgs);
        }

        /// <summary>
        /// Handles property changes in the parameters and updates the chat options.
        /// </summary>
        /// <param name="_">The sender of the event.</param>
        /// <param name="__">The event arguments.</param>
        private void Parameters_PropertyChanged(object? _, PropertyChangedEventArgs __) => OnChatOptionsChanged();

        /// <summary>
        /// Invokes the ChatOptionsChanged event with the current chat options.
        /// </summary>
        private void OnChatOptionsChanged()
        {
            ChatOptionsChanged?.Invoke(this, new(_selectedModel, _sendParameters ? _parameters : null, _useAgents ? _selectedAgent?.Id : null));
        }

        /// <summary>
        /// Handles the tick event of the loading animation timer, updating the assistant's loading message.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void LoadingAnimationTimer_Tick(object? sender, object e)
        {
            if (_lastAssistantMessage is not null)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    _loadingStep = (_loadingStep + 1) % 4;
                    _lastAssistantMessage.LoadingMessage = _lastAssistantMessage.Reasoning is not null && _lastAssistantMessage.Content is null ? "Pensando" : string.Empty;
                    _lastAssistantMessage.LoadingMessage += new string('.', _loadingStep);
                });
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>   
        /// Disposes the resources used by the ChatControl.
        /// </summary>
        /// <param name="disposing">Indicates whether the method is called from the Dispose method (true) or from the finalizer (false).</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _parameters.PropertyChanged -= Parameters_PropertyChanged;

                ModelSelector.Dispose();
                AgentSelector.Dispose();

                _cts?.Dispose();
            }
        }
    }

    /// <summary>
    /// Event arguments for chat options changes, including selected model, parameters, and agent ID.
    /// </summary>
    /// <param name="model">The selected AI model.</param>
    /// <param name="parameters">The parameters for the chat. Can be null.</param>
    /// <param name="agentId">The ID of the selected agent. Can be null.</param>
    public class ChatOptionsChangedEventArgs(AIModelViewModel? model, AIParametersViewModel? parameters, Guid? agentId) : EventArgs
    {
        public AIModelViewModel? SelectedModel { get; set; } = model;
        public AIParametersViewModel? Parameters { get; set; } = parameters;
        public Guid? AgentId { get; set; } = agentId;
    }
}