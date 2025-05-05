using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Text.Json;
using PowerPad.WinUI.Dialogs;
using PowerPad.WinUI.ViewModels.FileSystem;
using PowerPad.WinUI.ViewModels.Chat;
using PowerPad.Core.Models.FileSystem;
using Windows.System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using PowerPad.WinUI.Components.Controls;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml.Controls.Primitives;
using PowerPad.WinUI.Configuration;

namespace PowerPad.WinUI.Components.Editors
{
    public partial class ChatEditorControl : EditorControl
    {
        private DocumentViewModel _document;
        private ChatViewModel? _chat;

        public override bool IsDirty { get => _document.Status == DocumentStatus.Dirty; }

        public override DateTime LastSaveTime { get => _document.LastSaveTime; }

        public ChatEditorControl(Document document)
        {
            _document = new(document, this);
        }

        public override string GetContent(bool plainText = false)
        {
            return plainText
                ? string.Join('\n', _chat!.Messages.Select((Func<MessageViewModel, string>)(m => $"{m.Role}: {m.Content}")))
                : JsonSerializer.Serialize(_chat, typeof(ChatViewModel), AppJsonContext.Custom);
        }

        public override void SetContent(string content)
        {
            var error = false;

            if (!string.IsNullOrEmpty(content)) {
                try
                {
                    _chat = (ChatViewModel?)JsonSerializer.Deserialize(content, typeof(ChatViewModel), AppJsonContext.Custom);
                }
                catch (Exception)
                {
                    error = true;
                }
            }

            _chat ??= new() { Messages = [] };

            this.InitializeComponent();

            ChatControl.InitializeParameters(_chat.Model, _chat.Parameters, _chat.AgentId);
            
            if (_chat.Messages.Any()) UpdateLandingVisibility(showLanding: false);

            ChatControl.ChatOptionsChanged += ChatControl_ChatOptionsChanged;

            if (error)
            {
                DispatcherQueue.TryEnqueue(async () =>
                {
                    await Task.Delay(500);
                    await DialogHelper.Alert
                    (
                        XamlRoot,
                        "Error",
                        "No ha sido posible deserializar el contenido del chat."
                    );
                });
            }

            ItemsStackPanel_SizeChanged(null, null);
        }

        public override void SetFocus()
        {
            DispatcherQueue.TryEnqueue(async () =>
            {
                await Task.Delay(100);
                ChatControl.SetFocus();
            });
        }

        private void EditableTextBlock_Edited(object _, EventArgs __)
        {
            try
            {
                _document.RenameCommand.Execute(EditableTextBlock.Value);
            }
            catch
            {
                DispatcherQueue.TryEnqueue(async () =>
                {
                    await DialogHelper.Alert
                    (
                        XamlRoot,
                        "Error",
                        "No ha sido posible cambiar el nombre del documento."
                    );
                });
            }
        }

        public override void AutoSave()
        {
            //Chat elements always save automatically (instead of autosaving)
            _document.SaveCommand.Execute(null);
        }
        
        public override void Dispose()
        {
            _document = null!;
            _chat!.Messages.Clear();

            ChatControl.ChatOptionsChanged -= ChatControl_ChatOptionsChanged;
            ChatControl.Dispose();

            GC.SuppressFinalize(this);
        }

        private void InvertedListView_Loaded(object _, RoutedEventArgs __)
        {
            ItemsStackPanel? itemsStackPanel = FindElement<ItemsStackPanel>(InvertedListView);
            if (itemsStackPanel is not null)
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
                if (result is not null)
                {
                    return result;
                }
            }

            return null;
        }

        private void UpdateLandingVisibility(bool showLanding)
        {
            if (showLanding)
            {
                Landing.Visibility = Visibility.Visible;
                ChatGrid.VerticalAlignment = VerticalAlignment.Center;
                ChatRowDefinition.Height = new(0, GridUnitType.Auto);
                UndoButton.Visibility = Visibility.Collapsed;
                CleanButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                Landing.Visibility = Visibility.Collapsed;
                ChatGrid.VerticalAlignment = VerticalAlignment.Stretch;
                ChatRowDefinition.Height = new(1, GridUnitType.Star);
                UndoButton.Visibility = Visibility.Visible;
                CleanButton.Visibility = Visibility.Visible;
            }
        }

        private void ItemsStackPanel_SizeChanged(object? _, SizeChangedEventArgs? __)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                var scrollViewer = FindElement<ScrollViewer>(InvertedListView);

                if (scrollViewer is not null)
                {
                    var isScrollbarVisible = scrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible;

                    InvertedListView.Padding = InvertedListView.Padding with { Right = isScrollbarVisible ? 12 : -12 };
                }
            });
        }

        private async void ChatControl_SendButtonClicked(object _, RoutedEventArgs __)
        {
            if (Landing.Visibility == Visibility.Visible) UpdateLandingVisibility(showLanding: false);
            else
            {
                var scrollViewer = FindElement<ScrollViewer>(InvertedListView);

                if (scrollViewer is not null)
                {
                    while (Math.Ceiling(scrollViewer.VerticalOffset) < scrollViewer.ScrollableHeight)
                    {
                        scrollViewer.ChangeView(null, scrollViewer.ScrollableHeight + 100, null);
                        InvertedListView.UpdateLayout();

                        await Task.Delay(100);
                    }
                }
            }

            UndoButton.IsEnabled = false;
            CleanButton.IsEnabled = false;
            ChatControl.StartStreamingChat
            (
                _chat!.Messages,
                () =>
                {
                    _document.Status = DocumentStatus.Dirty;
                    UndoButton.IsEnabled = _chat.Messages.Any();
                    CleanButton.IsEnabled = _chat.Messages.Any();
                }
            );
        }

        private void ChatControl_ChatOptionsChanged(object? _, ChatOptionsChangedEventArgs eventArgs)
        {
            if (_chat!.Model != eventArgs.SelectedModel
                || _chat.Parameters != eventArgs.Parameters
                || _chat.AgentId != eventArgs.AgentId)
            {
                _chat.Model = eventArgs.SelectedModel;
                _chat.Parameters = eventArgs.Parameters?.Copy();
                _chat.AgentId = eventArgs.AgentId;
                _document.Status = DocumentStatus.Dirty;
            }
        }

        private void ChatControl_ParametersVisibilityChanged(object _, bool parametersPanelVisible)
        {
            //TODO: Error if it is called multiple times
            if (Landing.Visibility == Visibility.Visible)
            {
                LandingContent.Visibility = parametersPanelVisible ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public override int WordCount()
        {
            return _chat!.Messages.Sum(m => m.Content?.Split(' ')?.Length ?? 0);
        }

        private async void CopyButton_Click(object sender, RoutedEventArgs __)
        {
            var message = (MessageViewModel)((HyperlinkButton)sender).Tag;

            var dataPackage = new DataPackage();
            dataPackage.SetText(message.Content);
            Clipboard.SetContent(dataPackage);

            var flyout = new Flyout
            {
                Content = new TextBlock
                {
                    Text = "¡Copiado!",
                    Padding = new Thickness(0),
                    Margin = new Thickness(-6),
                    TextAlignment = TextAlignment.Center
                },
                Placement = FlyoutPlacementMode.Top
            };

            flyout.ShowAt((HyperlinkButton)sender);
            await Task.Delay(1000);
            flyout.Hide();
        }

        private async void UndoButton_Click(object _, RoutedEventArgs __)
        {
            var result = await DialogHelper.Confirm
            (
                XamlRoot, 
                "Eliminar último mensaje", 
                "Esta acción eliminará su último mensaje y la última respuesta del asistente.\n¿Está seguro?"
            );
            
            if (result == ContentDialogResult.Primary)
            {
                _chat!.RemoveLastMessageCommand.Execute(null);
                _document.Status = DocumentStatus.Dirty;

                UpdateLandingVisibility(showLanding: !_chat.Messages.Any());
            }
        }

        private async void CleanButton_Click(object _, RoutedEventArgs __)
        {
            var result = await DialogHelper.Confirm
            (
                XamlRoot, 
                "Eliminar la conversación", 
                "Esta acción eliminará toda la conversación. ¿Está seguro?"
            );

            if (result == ContentDialogResult.Primary)
            {
                _chat!.ClearMessagesCommand.Execute(null);
                _document.Status = DocumentStatus.Dirty;

                UpdateLandingVisibility(showLanding: true);
            }
        }
    }
}