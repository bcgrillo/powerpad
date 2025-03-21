using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PowerPad.Core.Models;
using WinUIEditor;
using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.WinUI.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using CommunityToolkit.WinUI;
using PowerPad.Core.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Collections.ObjectModel;
using static System.Net.Mime.MediaTypeNames;
using System.Threading;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PowerPad.WinUI.Components.Editors
{
    public sealed partial class ChatEditorControl : EditorControl
    {
        private CancellationTokenSource? _cts;
        private ScrollViewer? _scrollViewer;

        private DocumentViewModel _document;
        private ObservableCollection<MessageViewModel>? _messages;

        private IAIService _aiService;

        public override bool IsDirty { get => _document.Status == DocumentStatus.Dirty; }

        public override DateTime LastSaveTime { get => _document.LastSaveTime; }

        public ChatEditorControl(FolderEntryViewModel documentEntry, IAIService aiService)
        {
            this.InitializeComponent();

            _document = documentEntry.ToDocumentViewModel(this);
            _aiService = aiService;
        }

        public override string GetContent()
        {
            return JsonSerializer.Serialize(_messages);
        }

        public override void SetContent(string content)
        {
            List<MessageViewModel> chatMessages;

            try
            {
                chatMessages = JsonSerializer.Deserialize<List<MessageViewModel>>(content) ?? [];
            }
            catch (JsonException)
            {
                chatMessages = [];
                InfoBar.Title = "Error";
                InfoBar.Message = "No ha sido posible deserializar el contenido como JSON.";
                InfoBar.Visibility = Visibility.Visible;
                InfoBar.IsOpen = true;
            }

            _messages = [.. chatMessages];
        }

        private void EditableTextBlock_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            EditableTextBlock.Visibility = Visibility.Collapsed;
            EditableTextBox.Visibility = Visibility.Visible;
            EditableTextBox.Focus(FocusState.Programmatic);
        }

        private void EditableTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                FinalizeEditing();
            }
        }

        private void EditableTextBox_LostFocus(object sender, RoutedEventArgs e)
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
                InfoBar.Title = "Error";
                InfoBar.Message = "No ha sido posible cambiar el nombre del documento.";
                InfoBar.Visibility = Visibility.Visible;
                InfoBar.IsOpen = true;
            }
        }

        private void CopyBtn_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void InfoBar_Closing(InfoBar sender, InfoBarClosingEventArgs args)
        {
            InfoBar.Visibility = Visibility.Collapsed;
        }

        public override void AutoSave()
        {
            //Chat elements always save automatically (instead of autosaving)
            _document.SaveCommand.Execute(null);
        }

        public override void Dispose()
        {
            _document = null!;
            _messages = null;
        }

        private void SendBtn_Click(object sender, RoutedEventArgs e)
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

                await foreach (var messagePart in _aiService.GetStreamingResponse(history, cancellationToken: _cts.Token))
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

        private void InvertedListView_Loaded(object sender, RoutedEventArgs e)
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
