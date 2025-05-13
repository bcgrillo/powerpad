using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Models.FileSystem;
using PowerPad.WinUI.Components.Editors;
using PowerPad.WinUI.Helpers;
using PowerPad.WinUI.Messages;
using PowerPad.WinUI.ViewModels.FileSystem;
using System;
using System.Linq;

namespace PowerPad.WinUI.Components
{
    /// <summary>
    /// Represents a manager for handling editor controls and managing the workspace.
    /// </summary>
    public partial class EditorManager : UserControl, IRecipient<FolderEntryDeleted>
    {
        private const long AUTO_SAVE_INTERVAL = 3000;
        
        private static EditorManager? _activeInstance = null;
        private static readonly object _lock = new();

        private readonly WorkspaceViewModel _workspace;
        private readonly DispatcherTimer _timer;
        private EditorControl? _currentEditor;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorManager"/> class.
        /// </summary>
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

            SetActiveInstance(this);
        }

        /// <summary>
        /// Sets the active instance of the <see cref="EditorManager"/>.
        /// </summary>
        /// <param name="instance">The instance to set as active.</param>
        public static void SetActiveInstance(EditorManager instance)
        {
            lock (_lock)
            {
                if (_activeInstance is not null)
                    WeakReferenceMessenger.Default.Unregister<FolderEntryDeleted>(_activeInstance);

                WeakReferenceMessenger.Default.Register(instance);
                _activeInstance = instance;
            }
        }

        /// <summary>
        /// Opens a file in the editor manager.
        /// </summary>
        /// <param name="document">The document to open. If null, the editor will close the current file.</param>
        public void OpenFile(FolderEntryViewModel? document)
        {
            if (document is null)
            {
                _currentEditor?.Visibility = Visibility.Collapsed;
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

        /// <summary>
        /// Handles the receipt of a <see cref="FolderEntryDeleted"/> message.
        /// </summary>
        /// <param name="message">The message containing the folder entry that was deleted.</param>
        public void Receive(FolderEntryDeleted message)
        {
            var key = EditorManagerHelper.Editors
                .FirstOrDefault(kvp => kvp.Key.ModelEntry == message.Value).Key;

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

        /// <summary>
        /// Handles the click event for creating a new chat document.
        /// </summary>
        private void NewChatButton_Click(object _, RoutedEventArgs __)
        {
            _workspace.NewEntryCommand.Execute(NewEntryParameters.NewDocument(null, DocumentType.Chat));
        }

        /// <summary>
        /// Handles the click event for creating a new note document.
        /// </summary>
        private void NewNoteButton_Click(object _, RoutedEventArgs __)
        {
            _workspace.NewEntryCommand.Execute(NewEntryParameters.NewDocument(null, DocumentType.Note));
        }

        /// <summary>
        /// Handles the unload event of the user control.
        /// </summary>
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