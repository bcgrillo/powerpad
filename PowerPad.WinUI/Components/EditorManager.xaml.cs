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

        public EditorManager()
        {
            this.InitializeComponent();

            _editors = new Dictionary<FolderEntryViewModel, EditorControl>();
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
                    var newEditor = new TextEditorControl(document);
                    _editors.Add(document, newEditor);
                    EditorGrid.Children.Add(newEditor);
                    _currentEditor = newEditor;
                }
            }
        }
    }
}
