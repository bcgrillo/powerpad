using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using PowerPad.Core.Models;
using PowerPad.WinUI.Components.Editors;
using PowerPad.WinUI.Messages;
using PowerPad.WinUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using WinUIEditor;
using static PowerPad.WinUI.Configuration.ConfigConstants;

namespace PowerPad.WinUI.Components
{
    public sealed partial class EditorManager : UserControl, IRecipient<FolderEntryDeleted>
    {
        private const long AUTO_SAVE_INTERVAL = 3000;

        private EditorControl? _currentEditor;
        private readonly Dictionary<FolderEntryViewModel, EditorControl> _editors;
        private readonly DispatcherTimer _timer;

        public EditorManager()
        {
            this.InitializeComponent();

            _editors = [];

            _timer = new();
            _timer.Interval = TimeSpan.FromMilliseconds(AUTO_SAVE_INTERVAL);
            _timer.Tick += OnTimerTick;
            _timer.Start();

            WeakReferenceMessenger.Default.Register(this);
        }

        public void OpenFile(FolderEntryViewModel document)
        {
            _editors.TryGetValue(document, out EditorControl? _requestedEditor);

            if (_currentEditor != null && _currentEditor == _requestedEditor)
            {
                return;
            }
            else
            {
                if (_currentEditor != null)
                {
                    _currentEditor.Visibility = Visibility.Collapsed;
                }

                if (_requestedEditor != null)
                {
                    _requestedEditor.Visibility = Visibility.Visible;
                    _currentEditor = _requestedEditor;
                }
                else
                {
                    EditorControl newEditor;

                    if (document.DocumentType == DocumentTypes.Chat)
                    {
                        newEditor = new ChatEditorControl(document);
                    }
                    else
                    {
                        newEditor = new TextEditorControl(document);
                    }

                    newEditor.Visibility = Visibility.Collapsed;

                    _editors.Add(document, newEditor);
                    EditorGrid.Children.Add(newEditor);
                    _currentEditor = newEditor;

                    newEditor.Visibility = Visibility.Visible;
                }
            }
        }

        private void OnTimerTick(object? sender, object e)
        {
            var editorsToRemove = new List<FolderEntryViewModel>();

            foreach (var kvp in _editors)
            {
                var editor = kvp.Value;
                if (editor.IsDirty)
                {
                    editor.AutoSave();
                }
                else if ((DateTime.Now - editor.LastSaveTime).TotalMinutes > 10)
                {
                    editorsToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in editorsToRemove)
            {
                EditorGrid.Children.Remove(_editors[key]);
                _editors[key].Dispose();

                if (_currentEditor == _editors[key])
                {
                    _currentEditor = null;
                }

                _editors.Remove(key);
            }
        }

        public void Receive(FolderEntryDeleted message)
        {
            FolderEntryViewModel? key = null;

            foreach (var kvp in _editors)
            {
                if (kvp.Key.ModelEntry == message.Value) key = kvp.Key;
            }

            if (key != null)
            {
                EditorGrid.Children.Remove(_editors[key]);
                _editors[key].Dispose();

                if (_currentEditor == _editors[key])
                {
                    _currentEditor = null;
                }
                
                _editors.Remove(key);
            }
        }
    }
}