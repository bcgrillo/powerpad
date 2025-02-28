using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Models;
using PowerPad.WinUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Components.Editors
{
    public abstract class EditorControl : UserControl, IEditorControl
    {
        public EditorControl()
        {
        }

        public abstract string GetContent();
        public abstract void SetContent(string content);
    }
}
