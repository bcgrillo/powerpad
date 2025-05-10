using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Contracts;
using System;

namespace PowerPad.WinUI.Components.Editors
{
    public abstract class EditorControl : UserControl, IEditorContract, IDisposable
    {
        public bool IsActive { get; set; } = true;

        public abstract string GetContent(bool plainText = false);
        public abstract void SetContent(string content);
        public abstract void SetFocus();
        public abstract bool IsDirty { get; }
        public abstract void AutoSave();
        public abstract DateTime LastSaveTime { get; }
        public abstract int WordCount();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
    }
}