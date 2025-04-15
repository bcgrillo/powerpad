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
using System.Diagnostics;
using System.ComponentModel;

namespace PowerPad.WinUI.Components.Controls
{
    public partial class ChatControl : UserControl
    {
        private const double DEBOURCE_INTERVAL = 200;
        private const double LOADING_ANIMATION_INTERVAL = 200;

        private readonly IChatService _chatService;
        private readonly SettingsViewModel _settings;
        private readonly DispatcherTimer _debounceTimer;
        private readonly DispatcherTimer _loadingAnimationTimer;
        private readonly CancellationTokenSource _cts;

        private int _loadingStep = 0;
        private Action? _finalizeChatAction;
        private ICollection<MessageViewModel>? _messageList;
        private MessageViewModel? _lastMessage;

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

        public ChatControl()
        {
            this.InitializeComponent();

            _chatService = App.Get<IChatService>();
            _settings = App.Get<SettingsViewModel>();
            _cts = new();

            if (_settings.Models.DefaultModel is not null)
            {
                ModelIcon.Source = _settings.Models.DefaultModel.ModelProvider.GetIcon();
                ModelName.Text = _settings.Models.DefaultModel!.CardName;
                SetModelsMenu();
            }

            _parameters = _settings.Models.DefaultParameters.Copy();
            _parameters.PropertyChanged += Parameters_PropertyChanged;

            _debounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(DEBOURCE_INTERVAL)
            };
            _debounceTimer.Tick += DebounceTimer_Tick;

            _loadingAnimationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(LOADING_ANIMATION_INTERVAL)
            };
            _loadingAnimationTimer.Tick += LoadingAnimationTimer_Tick;

            _settings.Models.PropertyChanged += Models_PropertyChanged;
        }

        private void Models_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private void DebounceTimer_Tick(object? sender, object e)
        {
            _debounceTimer.Stop();
            SetModelsMenu();
        }

        public void SetFocus()
        {
            ChatInputBox.Focus(FocusState.Keyboard);
        }

        public void SetModel(AIModelViewModel? model)
        {
            _selectedModel = model;

            if (model is null)
            {
                ((RadioMenuFlyoutItem)ModelFlyoutMenu.Items.First()).IsChecked = true;
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
                    //Selected model is not available
                    _selectedModel = null;
                    ((RadioMenuFlyoutItem)ModelFlyoutMenu.Items.First()).IsChecked = true;
                    OnChatOptionChanged();
                }
            }

            UpdateModelButtonContent();
        }

        public void SetParameters(AIParametersViewModel? parameters)
        {
            if (parameters is not null)
            {
                _parameters.Set(parameters.GetRecord());

                if (!_sendParameters)
                {
                    _sendParameters = true;
                    EnableParametersSwitch.IsOn = true;
                    ToggleParameterVisibility();
                }
            }
            else if (_sendParameters)
            {
                _sendParameters = false;
                EnableParametersSwitch.IsOn = false;
                ToggleParameterVisibility();
            }
        }

        private void SetModelsMenu()
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

                var availableProviders = _settings.General.GetAvailableModelProviders();

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

        private void SetModelItem_Click(object sender, RoutedEventArgs _)
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
                ModelIcon.Source = _selectedModel.ModelProvider.GetIcon();
                ModelName.Text = _selectedModel.CardName;
            }
            else if (_settings.Models.DefaultModel is not null)
            {
                ModelIcon.Source = _settings.Models.DefaultModel.ModelProvider.GetIcon();
                ModelName.Text = _settings.Models.DefaultModel.CardName;
            }
            else
            {
                ModelIcon.Source = null;
                ModelName.Text = "Unavailable";
            }
        }

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

            messageList.Add(new(ChatInputBox.Text.Trim().Replace("\r", "  \r"), DateTime.Now, ChatRole.User));

            _ = Task.Run(async () =>
            {
                var history = messageList.Select(m => new ChatMessage(m.Role, m.Content)).ToList();
                _lastMessage = new MessageViewModel(string.Empty, DateTime.Now, ChatRole.Assistant);

                DispatcherQueue.TryEnqueue(() =>
                {
                    messageList.Add(_lastMessage);
                    SendButton.Visibility = Visibility.Collapsed;
                    StopButton.Visibility = Visibility.Visible;
                    ChatInputBox.IsReadOnly = true;
                    ModelButton.IsEnabled = false;
                    ParametersButton.IsEnabled = false;
                });

                _cts.TryReset();

                var parameters = _sendParameters ? _parameters
                    : (_settings.Models.SendDefaultParameters ? _settings.Models.DefaultParameters : null);

                await foreach (var messagePart in _chatService.GetChatResponse(history, _selectedModel?.GetRecord(), parameters?.GetRecord(), _cts.Token))
                {
                    try
                    {
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            _lastMessage.Content += messagePart.Text;
                            if (_lastMessage.Loading) _lastMessage.Loading = false;
                        });
                    }
                    catch (Exception ex)
                    {
                        //TODO: Anythig
                        Debug.WriteLine(ex.ToString());
                    }
                }

                if (!_cts.IsCancellationRequested) FinalizeChat();
            });
        }

        private void FinalizeChat()
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (_lastMessage is not null && string.IsNullOrWhiteSpace(_lastMessage.Content))
                    _messageList!.Remove(_lastMessage);

                StopButton.Visibility = Visibility.Collapsed;
                SendButton.Visibility = Visibility.Visible;
                ChatInputBox.Text = string.Empty;
                ChatInputBox.IsReadOnly = false;
                ModelButton.IsEnabled = true;
                ParametersButton.IsEnabled = true;
                ChatInputBox.Focus(FocusState.Keyboard);

                _finalizeChatAction?.Invoke();
                _finalizeChatAction = null;
                _messageList = null;
                _lastMessage = null;
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
            }
            else
            {
                _sendParameters = false;
                ControlDefaultParamters.Visibility = Visibility.Visible;
                ControlCustomParamters.Visibility = Visibility.Collapsed;
            }
        }

        private void CloseParametersButton_Click(object o, RoutedEventArgs eventArgs)
        {
            ParametersButton.IsChecked = false;
            ParametersButton_Click(o, eventArgs);
        }

        private void Parameters_PropertyChanged(object? _, PropertyChangedEventArgs __) => OnChatOptionChanged();

        private void OnChatOptionChanged() => ChatOptionsChanged?.Invoke(this, new(_selectedModel, _sendParameters ? _parameters : null));

        private void LoadingAnimationTimer_Tick(object? sender, object e)
        {
            if (_lastMessage is not null)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    _loadingStep = (_loadingStep + 1) % 4;
                    _lastMessage.LoadingMessage = new string('.', _loadingStep);
                });
            }
        }
    }

    public class ChatOptionChangedEventArgs(AIModelViewModel? model, AIParametersViewModel? parameters) : EventArgs
    {
        public AIModelViewModel? SelectedModel { get; set; } = model;
        public AIParametersViewModel? Parameters { get; set; } = parameters;
    }
}