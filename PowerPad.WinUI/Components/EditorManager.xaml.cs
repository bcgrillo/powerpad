using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Models.FileSystem;
using PowerPad.WinUI.Components.Editors;
using PowerPad.WinUI.Messages;
using PowerPad.WinUI.ViewModels.FileSystem;
using System;
using PowerPad.WinUI.Helpers;
using System.Threading;

namespace PowerPad.WinUI.Components
{
    public sealed partial class EditorManager : UserControl, IRecipient<FolderEntryDeleted>
    {
        private static EditorManager? _registredInstance = null;
        private readonly Lock _registredInstenceLock = new();

        private readonly WorkspaceViewModel _workspace;
        private const long AUTO_SAVE_INTERVAL = 3000;
        private EditorControl? _currentEditor;

        private readonly DispatcherTimer _timer;

        public EditorManager()
        {
            this.InitializeComponent();

            _workspace = App.Get<WorkspaceViewModel>();

            _timer = new()
            {
                Interval = TimeSpan.FromMilliseconds(AUTO_SAVE_INTERVAL)
            };
            _timer.Tick += (o, e) => EditorManagerHelper.AutoSaveEditors();
            _timer.Start();

            lock (_registredInstenceLock)
            {
                if (_registredInstance is not null)
                    WeakReferenceMessenger.Default.Unregister<FolderEntryDeleted>(_registredInstance);

                WeakReferenceMessenger.Default.Register(this);
                _registredInstance = this;
            }
        }

        public void OpenFile(FolderEntryViewModel? document)
        {
            if (document is null)
            {
                if (_currentEditor is not null) _currentEditor.Visibility = Visibility.Collapsed;
                _currentEditor = null;

                Landing.Visibility = Visibility.Visible;
                EditorGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                EditorManagerHelper.Editors.TryGetValue(document, out EditorControl? _requestedEditor);

                if (_currentEditor is not null && _currentEditor == _requestedEditor)
                {
                    return;
                }
                else
                {
                    if (_currentEditor is not null)
                    {
                        _currentEditor.IsActive = false;
                        EditorGrid.Children.Remove(_currentEditor);
                    }

                    if (_requestedEditor is not null)
                    {
                        _requestedEditor.IsActive = true;
                        EditorGrid.Children.Add(_requestedEditor);

                        _currentEditor = _requestedEditor;
                    }
                    else
                    {
                        EditorControl newEditor;

                        if (document.DocumentType == DocumentType.Chat)
                            newEditor = new ChatEditorControl((Document)document.ModelEntry);
                        else
                            newEditor = new TextEditorControl((Document)document.ModelEntry);

                        EditorManagerHelper.Editors.Add(document, newEditor);

                        EditorGrid.Children.Add(newEditor);
                        _currentEditor = newEditor;
                    }

                    if (Landing.Visibility == Visibility.Visible)
                    {
                        Landing.Visibility = Visibility.Collapsed;
                        EditorGrid.Visibility = Visibility.Visible;
                    }
                }

                _currentEditor.SetFocus();
            }
        }

        public void Receive(FolderEntryDeleted message)
        {
            FolderEntryViewModel? key = null;

            foreach (var kvp in EditorManagerHelper.Editors)
            {
                if (kvp.Key.ModelEntry == message.Value) key = kvp.Key;
            }

            if (key is not null)
            {
                var removedEditor = EditorManagerHelper.Editors[key];

                EditorGrid.Children.Remove(removedEditor);
                removedEditor.Dispose();
                EditorManagerHelper.Editors.Remove(key);

                if (_currentEditor == removedEditor)
                {
                    _currentEditor = null;
                    Landing.Visibility = Visibility.Visible;
                    EditorGrid.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void NewChatButton_Click(object _, RoutedEventArgs __)
        {
            _workspace.NewEntryCommand.Execute(NewEntryParameters.NewDocument(null, DocumentType.Chat));
        }

        private void NewNoteButton_Click(object _, RoutedEventArgs __)
        {
            _workspace.NewEntryCommand.Execute(NewEntryParameters.NewDocument(null, DocumentType.Text));
        }

        private void UserControl_Unloaded(object _, RoutedEventArgs __)
        {
            if (_currentEditor is not null)
            {
                _currentEditor.IsActive = false;
                EditorGrid.Children.Remove(_currentEditor);
            }
        }
    }
}