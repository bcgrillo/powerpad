using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using PowerPad.Core.Models;
using PowerPad.WinUI.Components.Editors;
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
        private Dictionary<FileItem, EditorControl> _editors;

        public EditorManager()
        {
            this.InitializeComponent();

            _editors = new Dictionary<FileItem, EditorControl>();
        }

        public void OpenFile(FileItem fileItem)
        {
            if(_currentEditor?.FileItem == fileItem)
            {
                return;
            }
            else
            {
                if (_currentEditor != null)
                {
                    _currentEditor.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                }

                if (_editors.ContainsKey(fileItem))
                {
                    _editors[fileItem].Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    _currentEditor = _editors[fileItem];
                }
                else {
                    var newEditor = new TextEditorControl(fileItem);
                    _editors.Add(fileItem, newEditor);
                    EditorGrid.Children.Add(newEditor);
                    _currentEditor = newEditor;
                }
            }
        }
    }
}
