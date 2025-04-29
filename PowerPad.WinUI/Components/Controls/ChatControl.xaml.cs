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
        private const double DEBOURCE_INTERVAL = 200;
        private const double LOADING_ANIMATION_INTERVAL = 200;
        private static readonly string[] THINK_START_TAG = ["<think>", "<thought>"];
        private static readonly string[] THINK_END_TAG = ["</think>", "</thought>"];
        private const string MARKDOWN_QUOTE = "> ";

        private readonly IChatService _chatService;
        private readonly SettingsViewModel _settings;
        private readonly AgentsCollectionViewModel _agentsCollection;
        private readonly DispatcherTimer _debounceTimer;
        private readonly DispatcherTimer _loadingAnimationTimer;
        private CancellationTokenSource _cts;

        private int _loadingStep = 0;
        private Action? _finalizeChatAction;
        private ICollection<MessageViewModel>? _messageList;
        private MessageViewModel? _lastUserMessage;
        private MessageViewModel? _lastAssistantMessage;

        public event EventHandler<RoutedEventArgs>? SendButtonClicked;
        public event EventHandler<ChatOptionChangedEventArgs>? ChatOptionsChanged;
        public event EventHandler<bool>? ParametersVisibilityChanged;

        public string ChatPlaceHolder
        {
            get => (string)GetValue(ChatPlaceHolderProperty);
            set => SetValue(ChatPlaceHolderProperty, value);
        }

        public static readonly DependencyProperty ChatPlaceHolderProperty =
            DependencyProperty.Register(nameof(ChatPlaceHolder), typeof(string), typeof(ChatControl), new(null));

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
            _agentsCollection = App.Get<AgentsCollectionViewModel>();
            _cts = new();

            _debounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(DEBOURCE_INTERVAL)
            };
            _debounceTimer.Tick += DebounceTimer_Tick;
            _debounceTimer.Stop();

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
            ModelIcon.UpdateEnabledLayout((bool)eventArgs.NewValue);
            ParametersIcon.UpdateEnabledLayout((bool)eventArgs.NewValue);
        }

        private void Models_PropertyChanged(object? _, EventArgs __)
        {
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private void DebounceTimer_Tick(object? _, object __)
        {
            _debounceTimer.Stop();
            UpdateModelsMenu();
        }

        public void InitializeParameters(AIModelViewModel? model, AIParametersViewModel? parameters, Guid? agentId)
        {
            _selectedModel = model;

            UpdateModelsMenu();

            if (parameters is not null)
            {
                _parameters.SetRecord(parameters.GetRecord());

                _sendParameters = true;
                EnableParametersSwitch.IsOn = true;
                ToggleParameterVisibility();
            }

            if (agentId.HasValue)
            {
                _selectedAgent = _agentsCollection.GetAgent(agentId.Value);
            }

            UpdateAgentsMenu();

            _parameters.PropertyChanged += Parameters_PropertyChanged;
            _settings.General.ProviderAvaibilityChanged += Models_PropertyChanged;
            _settings.Models.ModelAvaibilityChanged += Models_PropertyChanged;
            _settings.Models.DefaultModelChanged += DefaultModel_Changed;
            _agentsCollection.Agents.CollectionChanged += Agents_CollectionChanged;
        }

        public void SetFocus()
        {
            ChatInputBox.Focus(FocusState.Keyboard);
        }

        private void SetModel(AIModelViewModel? model)
        {
            _selectedModel = model;

            if (model is null)
            {
                DispatcherQueue.TryEnqueue(async () =>
                {
                    await Task.Delay(100);
                    if (ModelFlyoutMenu.Items.Any()) ((RadioMenuFlyoutItem)ModelFlyoutMenu.Items.First()).IsChecked = true;
                });
            }
            else
            {
                var menuItem = (RadioMenuFlyoutItem?)ModelFlyoutMenu.Items.FirstOrDefault(i => i.Tag as AIModelViewModel == model);

                if (menuItem is not null)
                {
                    DispatcherQueue.TryEnqueue(async () =>
                    {
                        await Task.Delay(100);
                        menuItem.IsChecked = true;
                    });
                }
                else
                {
                    _selectedModel = null;

                    menuItem = (RadioMenuFlyoutItem?)ModelFlyoutMenu.Items.FirstOrDefault();

                    if (menuItem is not null)
                    {
                        DispatcherQueue.TryEnqueue(async () =>
                        {
                            await Task.Delay(100);
                            menuItem.IsChecked = true;
                        });
                    }

                    OnChatOptionChanged();
                }
            }

            UpdateModelButtonContent();
        }

        private void UpdateModelsMenu()
        {
            ModelFlyoutMenu.Items.Clear();

            if (_settings.Models.DefaultModel is not null)
            {
                var firstItem = new RadioMenuFlyoutItem
                {
                    Text = $"Por defecto ({_settings.Models.DefaultModel!.CardName})",
                    Tag = null,
                    Icon = new ImageIcon() { Source = _settings.Models.DefaultModel!.ModelProvider.GetIcon() },
                };

                firstItem.Click += SetModelItem_Click;
                ModelFlyoutMenu.Items.Add(firstItem);
                ModelFlyoutMenu.Items.Add(new MenuFlyoutSeparator());

                var availableProviders = _settings.General.AvailableProviders.OrderBy(p => p);

                foreach (var provider in availableProviders)
                {
                    var elementAdded = false;

                    foreach (var item in _settings.Models.AvailableModels
                        .Where(m => m.ModelProvider == provider && m.Enabled)
                        .OrderBy(m => m.Name))
                    {
                        var menuItem = new RadioMenuFlyoutItem
                        {
                            Text = item.CardName,
                            Tag = item,
                            Icon = new ImageIcon() { Source = provider.GetIcon() }
                        };

                        ModelFlyoutMenu.Items.Add(menuItem);

                        menuItem.Click += SetModelItem_Click;

                        elementAdded = true;
                    }

                    if (elementAdded) ModelFlyoutMenu.Items.Add(new MenuFlyoutSeparator());
                }

                ModelFlyoutMenu.Items.RemoveAt(ModelFlyoutMenu.Items.Count - 1);

                SetModel(_selectedModel);
            }
        }

        private void SetModelItem_Click(object sender, RoutedEventArgs __)
        {
            _selectedModel = (AIModelViewModel?)((RadioMenuFlyoutItem)sender).Tag;

            UpdateModelButtonContent();

            ((RadioMenuFlyoutItem)sender).IsChecked = true;
            OnChatOptionChanged();
        }

        private void UpdateModelButtonContent()
        {
            if (_selectedModel is not null)
            {
                ModelName.Text = _selectedModel.CardName;
                ModelIcon.Source = _selectedModel.ModelProvider.GetIcon();
            }
            else if (_settings.Models.DefaultModel is not null)
            {
                ModelName.Text = _settings.Models.DefaultModel.CardName;
                ModelIcon.Source = _settings.Models.DefaultModel.ModelProvider.GetIcon();
            }
            else
            {
                ModelName.Text = "Unavailable";
                ModelIcon.Source = null;
            }
        }

        private void DefaultModel_Changed(object? _, EventArgs __)
        {
            if (_settings.Models.DefaultModel is not null && ModelFlyoutMenu.Items.Any())
            {
                var firstItem = (RadioMenuFlyoutItem)ModelFlyoutMenu.Items.First();

                firstItem.Text = $"Por defecto ({_settings.Models.DefaultModel!.CardName})";
                firstItem.Icon = new ImageIcon() { Source = _settings.Models.DefaultModel!.ModelProvider.GetIcon() };

                if (_selectedModel is null)
                {
                    ModelName.Text = _settings.Models.DefaultModel.CardName;
                    ModelIcon.Source = _settings.Models.DefaultModel.ModelProvider.GetIcon();

                    DispatcherQueue.TryEnqueue(async () =>
                    {
                        await Task.Delay(100);
                        firstItem.IsChecked = true;
                    });
                }
            }
        }

        private void UpdateAgentsMenu()
        {
            AgentFlyoutMenu.Items.Clear();

            var enabledAgents = _agentsCollection.Agents.Where(a => a.ShowInChats);

            if (enabledAgents.Any())
            {
                foreach (var agent in enabledAgents)
                {
                    var menuItem = new RadioMenuFlyoutItem
                    {
                        Text = agent.Name,
                        Tag = agent,
                        Icon = agent.IconElement
                    };

                    AgentFlyoutMenu.Items.Add(menuItem);

                    menuItem.Click += SetAgentItem_Click;
                }

                if (_selectedAgent is not null)
                {
                    var selectedAgentMenuItem = (RadioMenuFlyoutItem?)AgentFlyoutMenu.Items.FirstOrDefault(i => i.Tag as AgentViewModel == _selectedAgent);

                    if (selectedAgentMenuItem is null)
                    {
                        _selectedAgent = null;
                        OnChatOptionChanged();
                    }
                    else
                    {
                        DispatcherQueue.TryEnqueue(async () =>
                        {
                            await Task.Delay(100);
                            selectedAgentMenuItem.IsChecked = true;
                        });
                    }
                }

                _useAgents = _selectedAgent is not null;
                UpdateAgentButtonContent();
            }
            else
            {
                if (_selectedAgent is not null)
                {
                    _selectedAgent = null;
                    OnChatOptionChanged();
                }

                _useAgents = false;
                UpdateAgentButtonContent();

                AgentToggleButton.IsEnabled = false;
            }
        }

        private void SetAgentItem_Click(object sender, RoutedEventArgs __)
        {
            _selectedAgent = (AgentViewModel?)((RadioMenuFlyoutItem)sender).Tag;

            ((RadioMenuFlyoutItem)sender).IsChecked = true;

            UpdateAgentButtonContent();

            OnChatOptionChanged();
        }

        private void AgentToggleButton_Click(object sender, RoutedEventArgs __)
        {
            _useAgents = !_useAgents;

            if (_useAgents && _selectedAgent is null)
            {
                _selectedAgent = _agentsCollection.Agents.FirstOrDefault(a => a.ShowInChats);
                OnChatOptionChanged();
            }

            UpdateAgentButtonContent();
        }

        private void UpdateAgentButtonContent()
        {
            if (_useAgents)
            {
                AgentButton.Visibility = Visibility.Visible;
                ModelButton.Visibility = Visibility.Collapsed;
                ParametersButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                AgentButton.Visibility = Visibility.Collapsed;
                ModelButton.Visibility = Visibility.Visible;
                ParametersButton.Visibility = Visibility.Visible;
            }

            if (_selectedAgent is not null)
            {
                AgentIconControl.AgentIcon = _selectedAgent.Icon;
                AgentName.Visibility = Visibility.Visible;
                AgentName.Text = _selectedAgent.Name;
                ((Border)AgentToggleButton.Content).BorderBrush = (Brush)Application.Current.Resources["AccentFillColorDefaultBrush"];
            }
            else
            {
                AgentIconControl.AgentIcon = new("\uE99A", AgentIconType.FontIconGlyph);
                AgentName.Visibility = Visibility.Collapsed;
                AgentName.Text = null;
                ((Border)AgentToggleButton.Content).BorderBrush = new SolidColorBrush(Colors.Transparent);
            }
        }

        private void Agents_CollectionChanged(object? _, NotifyCollectionChangedEventArgs __) => UpdateAgentsMenu();

        private void ChatInputBox_TextChanged(object _, TextChangedEventArgs __)
        {
            SendButton.IsEnabled = !string.IsNullOrWhiteSpace(ChatInputBox.Text);
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
                    ModelButton.IsEnabled = false;
                    ParametersButton.IsEnabled = false;
                });

                _cts = new();

                var parameters = _sendParameters ? _parameters
                    : (_settings.Models.SendDefaultParameters ? _settings.Models.DefaultParameters : null);

                string messageBuffer = string.Empty;
                try
                {
                    var responseUpdates = _selectedAgent is null
                        ? _chatService.GetChatResponse(history, _selectedModel?.GetRecord(), parameters?.GetRecord(), _cts.Token)
                        : _chatService.GetAgentResponse(history, _selectedAgent.GetRecord(), _cts.Token);

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
                ModelButton.IsEnabled = true;
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
            OnChatOptionChanged();
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

        private void Parameters_PropertyChanged(object? _, PropertyChangedEventArgs __) => OnChatOptionChanged();

        private void OnChatOptionChanged() => ChatOptionsChanged?.Invoke(this, new(_selectedModel, _sendParameters ? _parameters : null, _selectedAgent?.Id));

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
            _settings.General.ProviderAvaibilityChanged -= Models_PropertyChanged;
            _settings.Models.ModelAvaibilityChanged -= Models_PropertyChanged;
            _settings.Models.DefaultModelChanged -= DefaultModel_Changed;
            _agentsCollection.Agents.CollectionChanged -= Agents_CollectionChanged;

            GC.SuppressFinalize(this);
        }
    }

    public class ChatOptionChangedEventArgs(AIModelViewModel? model, AIParametersViewModel? parameters, Guid? agentId) : EventArgs
    {
        public AIModelViewModel? SelectedModel { get; set; } = model;
        public AIParametersViewModel? Parameters { get; set; } = parameters;
        public Guid? AgentId { get; set; } = agentId;
    }
}