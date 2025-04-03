using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System.Text.Json;
using System.Collections.ObjectModel;
using PowerPad.WinUI.Dialogs;
using static PowerPad.Core.Constants;
using PowerPad.WinUI.ViewModels.FileSystem;
using PowerPad.WinUI.ViewModels.Chat;
using PowerPad.Core.Models.FileSystem;
using Windows.System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using PowerPad.WinUI.Components.Controls;

namespace PowerPad.WinUI.Components.Editors
{
    public sealed partial class ChatEditorControl : EditorControl
    {
        private DocumentViewModel _document;
        private ChatViewModel? _chat;

        public override bool IsDirty { get => _document.Status == DocumentStatus.Dirty; }

        public override DateTime LastSaveTime { get => _document.LastSaveTime; }

        public ChatEditorControl(Document document)
        {
            _document = new DocumentViewModel(document, this);
        }

        public override string GetContent()
        {
            return JsonSerializer.Serialize(_chat, JSON_SERIALIZER_OPTIONS);
        }

        public override void SetContent(string content)
        {
            var error = false;

            if (!string.IsNullOrEmpty(content)) {
                try
                {
                    _chat = JsonSerializer.Deserialize<ChatViewModel>(content, JSON_SERIALIZER_OPTIONS);
                }
                catch (JsonException)
                {
                    //TODO: Quitar esto
                    try
                    {
                        var chatMessages = JsonSerializer.Deserialize<List<MessageViewModel>>(content, JSON_SERIALIZER_OPTIONS);

                        _chat = new() { Messages = [.. chatMessages ?? []] };
                    }
                    catch (JsonException)
                    {
                        error = true;
                    }
                }
            }

            _chat ??= new() { Messages = [] };

            _chat.Messages.CollectionChanged += Messages_CollectionChanged;

            this.InitializeComponent();

            ChatControl.SetModel(_chat!.Model);
            ChatControl.SetParameters(_chat!.Parameters);
            if (_chat.Messages.Any()) HideLandingAndExpandChat();

            if (error)
            {
                DialogHelper.Alert
                (
                    this.XamlRoot,
                    "Error",
                    "No ha sido posible deserializar el contenido como JSON."
                ).Wait();
            }
        }

        public override void SetFocus() => ChatControl.SetFocus();

        #region ChatName
        private void EditableTextBlock_PointerPressed(object _, PointerRoutedEventArgs __)
        {
            EditableTextBlock.Visibility = Visibility.Collapsed;
            EditableTextBox.Visibility = Visibility.Visible;
            EditableTextBox.Focus(FocusState.Programmatic);
        }

        private void EditableTextBox_KeyDown(object _, KeyRoutedEventArgs args)
        {
            if (args.Key == VirtualKey.Enter)
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
        #endregion

        public override void AutoSave()
        {
            //Chat elements always save automatically (instead of autosaving)
            _document.SaveCommand.Execute(null);
        }

        public override void Dispose()
        {
            _document = null!;
            _chat!.Messages.Clear();
        }

        private void InvertedListView_Loaded(object _, RoutedEventArgs __)
        {
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

        private void Messages_CollectionChanged(object? _, NotifyCollectionChangedEventArgs __)
        {
            if (Landing.Visibility == Visibility.Visible && _chat!.Messages.Any()) HideLandingAndExpandChat();
        }

        private void HideLandingAndExpandChat()
        {
            Landing.Visibility = Visibility.Collapsed;
            ChatGrid.VerticalAlignment = VerticalAlignment.Stretch;
            ChatRowDefinition.Height = new GridLength(1, GridUnitType.Star);
        }

        private void ItemsStackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var scrollViewer = FindElement<ScrollViewer>(InvertedListView);

            if (scrollViewer != null)
            {
                bool isScrollbarVisible = scrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible;

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

        private async void ChatControl_SendButtonClicked(object _, RoutedEventArgs __)
        {
            var scrollViewer = FindElement<ScrollViewer>(InvertedListView);

            if (scrollViewer != null)
            {
                while (Math.Ceiling(scrollViewer.VerticalOffset) < scrollViewer.ScrollableHeight)
                {
                    scrollViewer.ChangeView(null, scrollViewer.ScrollableHeight + 100, null);
                    InvertedListView.UpdateLayout();

                    await Task.Delay(100);
                }
            }

            ChatControl.StartStreamingChat(_chat!.Messages, () => _document.Status = DocumentStatus.Dirty);
        }

        private void ChatControl_ChatOptionsChanged(object _, ChatOptionChangedEventArgs eventArgs)
        {
            if (_chat!.Model != eventArgs.SelectedModel || _chat!.Parameters != eventArgs.Parameters)
            {
                _chat!.Model = eventArgs.SelectedModel;
                _chat!.Parameters = eventArgs.Parameters;
                _document.Status = DocumentStatus.Dirty;
            }
        }
    }
}