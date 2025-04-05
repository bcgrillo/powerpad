using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Models.FileSystem;
using PowerPad.Core.Services.AI;
using PowerPad.WinUI.Components.Editors;
using PowerPad.WinUI.Messages;
using PowerPad.WinUI.ViewModels.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Web.AtomPub;

namespace PowerPad.WinUI.Components
{
    public sealed partial class EditorManager : UserControl, IRecipient<FolderEntryDeleted>
    {
        private readonly WorkspaceViewModel _workspace;
        private const long AUTO_SAVE_INTERVAL = 3000;
        private EditorControl? _currentEditor;

        private readonly Dictionary<FolderEntryViewModel, EditorControl> _editors;
        private readonly DispatcherTimer _timer;

        public event EventHandler? EditorUnloaded; //TODO: Remove?

        public EditorManager()
        {
            this.InitializeComponent();

            _workspace = App.Get<WorkspaceViewModel>();

            _editors = [];

            _timer = new()
            {
                Interval = TimeSpan.FromMilliseconds(AUTO_SAVE_INTERVAL)
            };
            _timer.Tick += (o, e) => AutoSaveEditors();
            _timer.Start();

            WeakReferenceMessenger.Default.Register(this);
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
                _editors.TryGetValue(document, out EditorControl? _requestedEditor);

                if (_currentEditor is not null && _currentEditor == _requestedEditor)
                {
                    return;
                }
                else
                {
                    if (_currentEditor is not null)
                    {
                        _currentEditor.Visibility = Visibility.Collapsed;
                    }

                    if (_requestedEditor is not null)
                    {
                        _requestedEditor.Visibility = Visibility.Visible;
                        _currentEditor = _requestedEditor;
                    }
                    else
                    {
                        EditorControl newEditor;

                        if (document.DocumentType == DocumentType.Chat)
                        {
                            newEditor = new ChatEditorControl((Document)document.ModelEntry);
                        }
                        else
                        {
                            newEditor = new TextEditorControl((Document)document.ModelEntry);
                        }

                        newEditor.Visibility = Visibility.Collapsed;

                        _editors.Add(document, newEditor);
                        EditorGrid.Children.Add(newEditor);
                        _currentEditor = newEditor;

                        Landing.Visibility = Visibility.Collapsed;
                        EditorGrid.Visibility = Visibility.Visible;

                        newEditor.Visibility = Visibility.Visible;
                    }
                }

                _currentEditor.SetFocus();
            }
        }

        private void AutoSaveEditors(bool forceSaveCurrent = false)
        {
            var editorsToRemove = new List<FolderEntryViewModel>();

            foreach (var kvp in _editors)
            {
                var editor = kvp.Value;
                if (editor.IsDirty || (forceSaveCurrent && _currentEditor == editor))
                {
                    editor.AutoSave();
                }
                else if ((DateTime.Now - editor.LastSaveTime).TotalMinutes > 10
                    && _currentEditor != editor)
                {
                    editorsToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in editorsToRemove)
            {
                EditorGrid.Children.Remove(_editors[key]);
                _editors[key].Dispose();
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

            if (key is not null)
            {
                EditorGrid.Children.Remove(_editors[key]);
                _editors[key].Dispose();

                if (_currentEditor == _editors[key])
                {
                    _currentEditor = null;
                    Landing.Visibility = Visibility.Visible;
                    EditorGrid.Visibility = Visibility.Collapsed;
                }
                
                _editors.Remove(key);
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

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            AutoSaveEditors(true);
        }
    }
}