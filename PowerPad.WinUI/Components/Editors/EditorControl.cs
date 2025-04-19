using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Contracts;
using System;

namespace PowerPad.WinUI.Components.Editors
{
    public abstract class EditorControl : UserControl, IEditorContract, IDisposable
    {
        public abstract string GetContent(bool plainText = false);
        public abstract void SetContent(string content);
        public abstract void SetFocus();
        public abstract bool IsDirty { get; }
        public abstract void AutoSave();
        public abstract DateTime LastSaveTime { get; }
        public abstract void Dispose();
        public abstract int WordCount();
        public bool IsActive { get; set; } = true;
    }
}