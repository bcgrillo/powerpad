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
    public abstract class EditorControl : UserControl, IEditorContract, IDisposable
    {
        public abstract string GetContent();
        public abstract void SetContent(string content);
        public abstract bool IsDirty { get; }
        public abstract void AutoSave();
        public abstract DateTime LastSaveTime { get; }
        public abstract void Dispose();
    }
}
