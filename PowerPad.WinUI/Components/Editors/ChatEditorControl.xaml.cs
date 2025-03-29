using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using PowerPad.WinUI.ViewModels;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Collections.ObjectModel;
using System.Threading;
using PowerPad.WinUI.Dialogs;
using static PowerPad.Core.Constants;
using PowerPad.WinUI.ViewModels.FileSystem;
using PowerPad.WinUI.ViewModels.Chat;
using PowerPad.WinUI.Helpers;
using PowerPad.Core.Models.FileSystem;
using PowerPad.Core.Services.AI;

namespace PowerPad.WinUI.Components.Editors
{
    public sealed partial class ChatEditorControl : EditorControl
    {
        private CancellationTokenSource? _cts;
        private ScrollViewer? _scrollViewer;

        private DocumentViewModel _document;
        private readonly ObservableCollection<MessageViewModel> _messages;

        private readonly IChatService _chatService;

        public override bool IsDirty { get => _document.Status == DocumentStatus.Dirty; }

        public override DateTime LastSaveTime { get => _document.LastSaveTime; }

        public ChatEditorControl(FolderEntryViewModel documentEntry, IChatService aiService)
        {
            this.InitializeComponent();

            _document = documentEntry.ToDocumentViewModel(this);
            _chatService = aiService;
            _messages = [];
        }

        public override string GetContent()
        {
            return JsonSerializer.Serialize(_messages, JSON_SERIALIZER_OPTIONS);
        }

        public override void SetContent(string content)
        {
            List<MessageViewModel>? chatMessages = null;

            if (!string.IsNullOrEmpty(content)) {
                try
                {
                    chatMessages = JsonSerializer.Deserialize<List<MessageViewModel>>(content);
                }
                catch (JsonException)
                {
                    DialogHelper.Alert
                    (
                        this.XamlRoot,
                        "Error",
                        "No ha sido posible deserializar el contenido como JSON."
                    ).Wait();
                }
            }

            chatMessages ??= [];

            _messages.AddRange(chatMessages);
        }

        private void EditableTextBlock_PointerPressed(object _, PointerRoutedEventArgs __)
        {
            EditableTextBlock.Visibility = Visibility.Collapsed;
            EditableTextBox.Visibility = Visibility.Visible;
            EditableTextBox.Focus(FocusState.Programmatic);
        }

        private void EditableTextBox_KeyDown(object _, KeyRoutedEventArgs args)
        {
            if (args.Key == Windows.System.VirtualKey.Enter)
            {
                FinalizeEditing();
            }
        }

        private void EditableTextBox_LostFocus(object _, RoutedEventArgs __)
        {
            FinalizeEditing();
        }

        private void FinalizeEditing()
        {
            EditableTextBlock.Visibility = Visibility.Visible;
            EditableTextBox.Visibility = Visibility.Collapsed;

            try
            {
                _document.RenameCommand.Execute(EditableTextBox.Text);
            }
            catch (Exception)
            {
                EditableTextBox.Text = _document.Name;

                DialogHelper.Alert
                (
                    this.XamlRoot,
                    "Error",
                    "No ha sido posible cambiar el nombre del documento."
                ).Wait();
            }
        }

        private void CopyBtn_Click(object _, RoutedEventArgs __)
        {
           
        }

        public override void AutoSave()
        {
            //Chat elements always save automatically (instead of autosaving)
            _document.SaveCommand.Execute(null);
        }

        public override void Dispose()
        {
            _document = null!;
            _messages.Clear();
        }

        private void SendBtn_Click(object _, RoutedEventArgs __)
        {
            _messages!.Add(new MessageViewModel(InputBox.Text.Trim(), DateTime.Now, ChatRole.User));

            _ = Task.Run(async () =>
            {
                var history = _messages.Select(m => new ChatMessage(m.Role, m.Content)).ToList();
                var responseMessage = new MessageViewModel(string.Empty, DateTime.Now, ChatRole.Assistant);

                DispatcherQueue.TryEnqueue(() =>
                {
                    _messages.Add(responseMessage);
                    StopBtn.Visibility = Visibility.Visible;
                    InputBox.IsEnabled = false;
                    InputBox.PlaceholderText = "Please wait for the response to complete before entering a new prompt";
                });

                _cts = new CancellationTokenSource();

                history.Insert(0, new ChatMessage(ChatRole.System, "You are a helpful assistant"));

                await foreach (var messagePart in _chatService.GetStreamingResponse(history, cancellationToken: _cts.Token))
                {
                    try
                    {
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            responseMessage.Content += messagePart.Text;
                        });
                    }
                    catch(Exception ex)
                    {
                        //TODO: Anythig
                        Console.WriteLine(ex.ToString());
                    }
                }

                _cts.Dispose();
                _cts = null;

                DispatcherQueue.TryEnqueue(() =>
                {
                    StopBtn.Visibility = Visibility.Collapsed;
                    SendBtn.Visibility = Visibility.Visible;
                    InputBox.Text = string.Empty;
                    InputBox.IsEnabled = true;
                });

                _document.Status = DocumentStatus.Dirty;
            });

            _document.Status = DocumentStatus.Dirty;
        }

        private void InvertedListView_Loaded(object _, RoutedEventArgs __)
        {
            _scrollViewer = FindElement<ScrollViewer>(InvertedListView);

            ItemsStackPanel? itemsStackPanel = FindElement<ItemsStackPanel>(InvertedListView);
            if (itemsStackPanel != null)
            {
                itemsStackPanel.SizeChanged += ItemsStackPanel_SizeChanged;
            }
        }

        private T? FindElement<T>(DependencyObject element)
        where T : DependencyObject
        {
            if (element is T targetElement)
            {
                return targetElement;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                var result = FindElement<T>(child);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private void ItemsStackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_scrollViewer != null)
            {
                bool isScrollbarVisible = _scrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible;

                if (isScrollbarVisible)
                {
                    InvertedListView.Padding = new Thickness(-12, 0, 12, 24);
                }
                else
                {
                    InvertedListView.Padding = new Thickness(-12, 0, -12, 24);
                }
            }
        }
    }
}
