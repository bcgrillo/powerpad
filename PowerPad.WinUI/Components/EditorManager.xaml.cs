using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using PowerPad.Core.Models;
using PowerPad.WinUI.Components.Editors;
using PowerPad.WinUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using WinUIEditor;

namespace PowerPad.WinUI.Components
{
    public sealed partial class EditorManager : UserControl
    {
        private EditorControl? _currentEditor;
        private Dictionary<FolderEntryViewModel, EditorControl> _editors;
        private DispatcherTimer _timer;

        public EditorManager()
        {
            this.InitializeComponent();

            _editors = new Dictionary<FolderEntryViewModel, EditorControl>();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(3);
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        public void OpenFile(FolderEntryViewModel document)
        {
            if(_currentEditor?.DataContext == document)
            {
                return;
            }
            else
            {
                if (_currentEditor != null)
                {
                    _currentEditor.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                }

                if (_editors.ContainsKey(document))
                {
                    _editors[document].Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    _currentEditor = _editors[document];
                }
                else {
                    var newEditor = new TextEditorControl(document)
                    {
                        Visibility = Microsoft.UI.Xaml.Visibility.Collapsed
                    };
                    _editors.Add(document, newEditor);
                    EditorGrid.Children.Add(newEditor);
                    _currentEditor = newEditor;

                    newEditor.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                }
            }
        }

        private void OnTimerTick(object sender, object e)
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
                _editors[key].Dispose();
                _editors.Remove(key);
            }
        }
    }
}
