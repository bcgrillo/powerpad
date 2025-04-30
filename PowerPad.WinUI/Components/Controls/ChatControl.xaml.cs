using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using PowerPad.WinUI.ViewModels.Chat;
using System.Threading.Tasks;
using System;
using Windows.System;
using Windows.UI.Core;
using System.Collections.Generic;
using Microsoft.Extensions.AI;
using System.Threading;
using System.Linq;
using PowerPad.Core.Services.AI;
using PowerPad.WinUI.ViewModels.Settings;
using PowerPad.WinUI.Helpers;
using PowerPad.WinUI.ViewModels.AI;
using System.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using PowerPad.WinUI.ViewModels.Agents;
using System.Collections.Specialized;
using PowerPad.Core.Models.AI;

namespace PowerPad.WinUI.Components.Controls
{
    public partial class ChatControl : UserControl, IDisposable
    {
        private const double LOADING_ANIMATION_INTERVAL = 200;
        private static readonly string[] THINK_START_TAG = ["<think>", "<thought>"];
        private static readonly string[] THINK_END_TAG = ["</think>", "</thought>"];
        private const string MARKDOWN_QUOTE = "> ";

        private readonly IChatService _chatService;
        private readonly SettingsViewModel _settings;
        private readonly DispatcherTimer _loadingAnimationTimer;

        private CancellationTokenSource _cts;

        private int _loadingStep = 0;
        private Action? _finalizeChatAction;
        private ICollection<MessageViewModel>? _messageList;
        private MessageViewModel? _lastUserMessage;
        private MessageViewModel? _lastAssistantMessage;

        public event EventHandler<RoutedEventArgs>? SendButtonClicked;
        public event EventHandler<ChatOptionsChangedEventArgs>? ChatOptionsChanged;
        public event EventHandler<bool>? ParametersVisibilityChanged;

        private AIModelViewModel? _selectedModel;
        private readonly AIParametersViewModel _parameters;
        private bool _sendParameters;
        private AgentViewModel? _selectedAgent;
        private bool _useAgents;

        public ChatControl()
        {
            this.InitializeComponent();

            _chatService = App.Get<IChatService>();
            _settings = App.Get<SettingsViewModel>();
            _cts = new();

            _loadingAnimationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(LOADING_ANIMATION_INTERVAL)
            };
            _loadingAnimationTimer.Tick += LoadingAnimationTimer_Tick;

            _parameters = _settings.Models.DefaultParameters.Copy();

            IsEnabledChanged += OnEnabledChanged;
        }

        private void OnEnabledChanged(object? _, DependencyPropertyChangedEventArgs eventArgs)
        {
            ModelSelector.UpdateEnabledLayout((bool)eventArgs.NewValue);
            ParametersIcon.UpdateEnabledLayout((bool)eventArgs.NewValue);
        }

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

        private void SelectedModel_Changed(object? _, EventArgs __)
        {
            if (_selectedModel != ModelSelector.SelectedModel)
            {
                _selectedModel = ModelSelector.SelectedModel;
                OnChatOptionsChanged();
            }
        }

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

        public void SetFocus()
        {
            ChatInputBox.Focus(FocusState.Keyboard);
        }

        private void AgentToggleButton_Click(object sender, RoutedEventArgs __)
        {
            _useAgents = !_useAgents;

            OnChatOptionsChanged();

            UpdateChatButtonsLayout();

            if (_useAgents && _selectedAgent is null) AgentSelector.ShowMenu();
        }

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
                ParametersButton.Visibility = Visibility.Visible;
                ((Border)AgentToggleButton.Content).BorderBrush = new SolidColorBrush(Colors.Transparent);
            }
        }

        private void ChatInputBox_TextChanged(object _, TextChangedEventArgs __)
        {
            SendButton.IsEnabled = !string.IsNullOrWhiteSpace(ChatInputBox.Text)
                && (!_useAgents || _selectedAgent is not null);

            if (string.IsNullOrEmpty(ChatInputBox.Text)) ChatInputBox.AcceptsReturn = false;
        }

        private void SendBtn_Click(object _, RoutedEventArgs e)
        {
            SendButtonClicked?.Invoke(this, e);
        }

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

                string messageBuffer = string.Empty;
                try
                {
                    var responseUpdates = _useAgents
                        ? _chatService.GetAgentResponse(history, _selectedAgent!.GetRecord(), _cts.Token)
                        : _chatService.GetChatResponse(history, _selectedModel?.GetRecord(), _sendParameters ? parameters?.GetRecord() : null, _cts.Token);

                    await foreach (var update in responseUpdates)
                    {
                        messageBuffer += update.Text;

                        string? reasoning = null;
                        string? content = null;
                        bool loading = true;

                        var thinkStartTag = THINK_START_TAG.FirstOrDefault(tag => messageBuffer.Contains(tag));

                        // Logic to parse and update reasoning and content from the message buffer
                        if (thinkStartTag is not null)
                        {
                            var thinkEndTag = THINK_END_TAG.FirstOrDefault(tag => messageBuffer.Contains(tag));

                            var startIndex = messageBuffer.IndexOf(thinkStartTag) + thinkStartTag.Length;

                            if (thinkEndTag is not null)
                            {
                                var endIndex = messageBuffer.IndexOf(thinkEndTag, startIndex);

                                reasoning = messageBuffer[startIndex..endIndex].Trim().Replace("\n\n", "\n");
                                content = messageBuffer[(endIndex + thinkEndTag.Length)..].Trim();

                                if (loading) loading = false;
                            }
                            else
                            {
                                reasoning = messageBuffer[startIndex..].Trim().Replace("\n\n", "\n");
                            }
                        }
                        else
                        {
                            content = messageBuffer;
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
                catch (TaskCanceledException) { }
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

        private void StopBtn_Click(object _, RoutedEventArgs __)
        {
            _cts.Cancel();
            FinalizeChat();
        }

        private void ChatInputBox_KeyDown(object _, KeyRoutedEventArgs e)
        {
            if (!ChatInputBox.IsReadOnly)
            {
                if (e.Key == VirtualKey.Enter)
                {
                    if (!InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift)
                        .HasFlag(CoreVirtualKeyStates.Down) &&
                        !string.IsNullOrWhiteSpace(ChatInputBox.Text))
                    {
                        SendButtonClicked?.Invoke(this, e);
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
        }

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

        private void EnableParametersSwitch_Toggled(object _, RoutedEventArgs __)
        {
            ToggleParameterVisibility();
            OnChatOptionsChanged();
        }

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

        private void CloseParametersButton_Click(object o, RoutedEventArgs eventArgs)
        {
            ParametersButton.IsChecked = false;
            ParametersButton_Click(o, eventArgs);
        }

        private void Parameters_PropertyChanged(object? _, PropertyChangedEventArgs __) => OnChatOptionsChanged();

        private void OnChatOptionsChanged()
        {
            ChatOptionsChanged?.Invoke(this, new(_selectedModel, _sendParameters ? _parameters : null, _useAgents ? _selectedAgent?.Id : null));
        }

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

        public void Dispose()
        {
            _parameters.PropertyChanged -= Parameters_PropertyChanged;

            ModelSelector.Dispose();
            AgentSelector.Dispose();

            GC.SuppressFinalize(this);
        }
    }

    public class ChatOptionsChangedEventArgs(AIModelViewModel? model, AIParametersViewModel? parameters, Guid? agentId) : EventArgs
    {
        public AIModelViewModel? SelectedModel { get; set; } = model;
        public AIParametersViewModel? Parameters { get; set; } = parameters;
        public Guid? AgentId { get; set; } = agentId;
    }
}